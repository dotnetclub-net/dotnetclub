using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Discussion.Web.Tests.Stubs
{
    public class StubHttpClient : HttpMessageInvoker
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

        public StubHttpClient Json(string path, object jsonObject)
        {
            return When(req =>
            {
                if (req.RequestUri.PathAndQuery != path) return null;
                
                var json = JsonConvert.SerializeObject(jsonObject);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json)
                    {
                        Headers =
                        {
                            ContentType = new MediaTypeHeaderValue("application/json")
                        }
                    }
                };

            });
        }
        
        public StubHttpClient StatusCode(string path, HttpStatusCode statusCode)
        {
            return When(req =>
            {
                if (req.RequestUri.PathAndQuery != path) return null;
                
                return new HttpResponseMessage(statusCode);
            });
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
                    if (response != null)
                        return Task.FromResult(response);
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound){ Content = new StringContent("")});
            }
        }
    }
}