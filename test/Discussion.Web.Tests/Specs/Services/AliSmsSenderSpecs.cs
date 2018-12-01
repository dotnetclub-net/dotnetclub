using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Discussion.Core.Communication.Sms.SmsSenders;
using Discussion.Core.Time;
using Discussion.Core.Utilities;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    public class AliSmsSenderSpecs
    {
        [Fact]
        public async Task should_send_post_request_to_ali_sms_service()
        {
            var (smsSender, httpClient) = CreateSmsSender();

            var verificationCode = StringUtility.RandomNumbers();
            await smsSender.SendVerificationCodeAsync("13603503455", verificationCode);

            var actualRequest = httpClient.RequestsSent.FirstOrDefault();
            Assert.NotNull(actualRequest);
            Assert.Equal("https://dysmsapi.aliyuncs.com/", actualRequest.RequestUri.ToString());
            Assert.Equal(HttpMethod.Post, actualRequest.Method);
            Assert.IsType<StringContent>(actualRequest.Content);
        }

        private static (AliyunSmsSender, StubHttpClient) CreateSmsSender()
        {
            var options = new AliyunSmsOptions()
            {
                AccessKeySecret = "dummy-secret",
                AccountKeyId = "dummy-key",
                SmsServiceSignName = "dotnetclub.net",
                SmsServiceTemplateCode = "SA873dK"
            };
            var optionsMock = new Mock<IOptions<AliyunSmsOptions>>();
            optionsMock.SetupGet(o => o.Value).Returns(options);

            var client = Services.StubHttpClient.Create().When(req =>
            {
                var response = new AliyunSmsResponse()
                {
                    RequestId = Guid.NewGuid().ToString(),
                    Code = "OK",
                    Message = "Sms Sent",
                    BizId = Guid.NewGuid().ToString()
                };
                return JsonResponse(response);
            });

            var smsSender = new AliyunSmsSender(optionsMock.Object, client, new SystemClock());
            return (smsSender, client);
        }

        private static HttpResponseMessage JsonResponse(object obj)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(obj))
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };
        }

        class AliyunSmsResponse
        {
            public string RequestId { get; set; }
            public string Code { get; set; }
            public string Message { get; set; }
            public string BizId { get; set; }
        }
    }

    class StubHttpClient : HttpMessageInvoker
    {
        readonly List<HttpRequestMessage> _sentRequests = new List<HttpRequestMessage>();
        readonly List<Func<HttpRequestMessage, HttpResponseMessage>> _stubs = new List<Func<HttpRequestMessage, HttpResponseMessage>>();
        private StubHttpClient(HttpMessageHandler handler) : base(handler)
        {
            
        }


        public StubHttpClient When(Func<HttpRequestMessage, HttpResponseMessage> stub)
        {
            _stubs.Add(stub);
            return this;
        }

        public static StubHttpClient Create()
        {
            var handler = new DummyHttpMessageHandler(); 
            var instance = new StubHttpClient(handler);
            handler.HttpClient = instance;
            
            return instance;
        }

        public IReadOnlyCollection<HttpRequestMessage> RequestsSent => _sentRequests.AsReadOnly();


        class DummyHttpMessageHandler : HttpMessageHandler
        {
            
            public StubHttpClient HttpClient { get; set; }


            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                HttpClient._sentRequests.Add(request);
                foreach (var handler in HttpClient._stubs)
                {
                    var response = handler(request);
                    if (request != null)
                        return Task.FromResult(response);
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }
        }
    }
}