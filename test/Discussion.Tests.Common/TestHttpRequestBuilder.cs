using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Tests.Common.AssertionExtensions;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Discussion.Tests.Common
{
    public class TestHttpRequestBuilder
    {
        private readonly TestApplication _app;
        private readonly string _path;


        private HttpMethod _method = HttpMethod.Get;
        
        private readonly List<Cookie> _cookies = new List<Cookie>();
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> _postForm;
        private byte[] _postEntity;
        private string _contentType;
        
        
        
        public TestHttpRequestBuilder(TestApplication app, string path)
        {
            _app = app;
            _path = path;
        }

        public TestHttpRequestBuilder Get()
        {
            _method = HttpMethod.Get;
            return this;
        }
        
        public TestHttpRequestBuilder Post()
        {
            _method = HttpMethod.Post;
            return this;
        }
        
        public TestHttpRequestBuilder WithCookie(string name, string value)
        {
            _cookies.Add(new Cookie(name, value));
            return this;
        }
        
        public TestHttpRequestBuilder WithForm(Dictionary<string, string> dic)
        {
            if (_postEntity != null)
            {
                throw new InvalidOperationException("Should not post form together with json");
            }

            _contentType = "application/x-www-form-urlencoded";
            _postForm = dic;
            return this;
        }
        
        public TestHttpRequestBuilder WithForm(object obj)
        {
            var anonymousType = obj.GetType();
            var props = anonymousType.GetProperties();
            _contentType = "application/x-www-form-urlencoded";
            _postForm = props
                .Select(x => (x.Name, x.GetValue(obj, null)))
                .ToDictionary(prop => prop.Item1, prop => prop.Item2.ToString());
            return this;
        }
        
        public TestHttpRequestBuilder WithJson(object obj)
        {
            if (_postForm != null)
            {
                throw new InvalidOperationException("Should not post json together with form");
            }
            
            
            var json = JsonConvert.SerializeObject(obj);
            _postEntity = Encoding.UTF8.GetBytes(json);
            _contentType = "application/json";
            return this;
        }
        
        public TestHttpRequestBuilder WithHeader(string name, string value)
        {
            _headers[name] = value;
            return this;
        }
        
        public ResponseAssertion ShouldSuccess(User user = null)
        {
            var response = SendRequest();
            var assertion = new ResponseAssertion(response, this);
            return assertion.WithResponse(res => response.IsSuccessStatusCode.ShouldEqual(true));
        }
        
        public ResponseAssertion ShouldSuccessWithRedirect(User user = null)
        {
            var response = SendRequest();
            var assertion = new ResponseAssertion(response, this);
            return assertion.WithResponse(res =>
            {
                var isRedirect = response.StatusCode == HttpStatusCode.Found ||
                                 response.StatusCode == HttpStatusCode.Moved;
                var isSignin = res.Headers.Location?.ToString()?.Contains("signin");
                isRedirect.ShouldEqual(true);
                (isSignin == null || !isSignin.Value).ShouldEqual(true);
            });
        }
        
        public ResponseAssertion ShouldFail(User user = null)
        {
            var response = SendRequest();
            var assertion = new ResponseAssertion(response, this);
            return assertion.WithResponse(res => response.IsSuccessStatusCode.ShouldEqual(false));
        }

        private HttpResponseMessage SendRequest()
        {
            var requestBuilder = new RequestBuilder(_app.Server, _path);
            
            if (_method == HttpMethod.Post)
            {
                var tokens = AntiForgeryRequestTokens.GetFromApplication(_app);
                if (_postForm != null)
                {
                    var dic = new Dictionary<string, string>(_postForm);

                    requestBuilder.And(req =>
                        {
                            dic["__RequestVerificationToken"] = tokens.VerificationToken;
                            var content = new FormUrlEncodedContent(dic);
                            content.Headers.ContentType = new MediaTypeHeaderValue(_contentType);
                            req.Content = content;
                        })
                        .WithCookie(tokens.Cookie);
                }
                else
                {
                    requestBuilder.AddHeader("RequestVerificationToken", tokens.VerificationToken);
                    if (_postEntity != null)
                    {
                        requestBuilder.And(req =>
                        {
                            var content = new ByteArrayContent(_postEntity);
                            content.Headers.ContentType = new MediaTypeHeaderValue(_contentType);
                            req.Content = content;
                        });
                    }
                    requestBuilder.WithCookie(tokens.Cookie);
                }
            }

            if (_headers.Any())
            {
                _headers.Keys.ToList().ForEach(key => requestBuilder.AddHeader(key, _headers[key]));
            }

            var response = requestBuilder.SendAsync(_method.ToString().ToUpper()).Result;
            
            _app.ResetUser();
            return response;
        }


        public class ResponseAssertion
        {
            private readonly HttpResponseMessage _response;

            public ResponseAssertion(HttpResponseMessage response, TestHttpRequestBuilder requestBuilder)
            {
                _response = response;
                And = requestBuilder;
            }

            public ResponseAssertion WithResponse(Action<HttpResponseMessage> assertion)
            {
                assertion(_response);
                return this;
            }
            
            public ResponseAssertion WithResponse(Func<HttpResponseMessage, bool> assertion)
            {
                assertion(_response).ShouldEqual(true);
                return this;
            }
            
            public ResponseAssertion WithJson<T>(Action<T> assertion)
            {
                Assert.Equal("application/json", _response.Content.Headers.ContentType.MediaType);
                var jsonContent = _response.ReadAllContent();
                assertion(JsonConvert.DeserializeObject<T>(jsonContent));
                return this;
            }
            
            
            public ResponseAssertion WithApiResult(Action<ApiResponse, JObject> assertion)
            {
                return WithJson<ApiResponse>(api =>
                {
                    assertion(api, api.Result as JObject);
                });
            }
            
            public ResponseAssertion WithSigninRedirect()
            {
                return WithResponse(res => res.StatusCode == HttpStatusCode.Redirect
                                           && res.Headers.Location.ToString().Contains("signin"));
            }

            public TestHttpRequestBuilder And { get; }
        }
        
        
        
    }
    
    public enum PrincipalStatus
    {
        OnlyAuthorized,
        AllUsers
    }
}