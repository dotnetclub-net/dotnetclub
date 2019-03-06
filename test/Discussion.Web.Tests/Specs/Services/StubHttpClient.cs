using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Discussion.Web.Tests.Specs.Services
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