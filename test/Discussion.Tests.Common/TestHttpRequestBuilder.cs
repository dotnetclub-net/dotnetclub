using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Tests.Common.AssertionExtensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

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
        
        public TestHttpRequestBuilder WithCookieFrom(HttpResponseMessage prevResponse)
        {
            if (!prevResponse.Headers.TryGetValues(HeaderNames.SetCookie, out var cookies))
            {
                return this;
            }
            
            var responseCookieHeaders = SetCookieHeaderValue.ParseList(cookies.ToList()); 
            foreach (var cookie in responseCookieHeaders)
            {
                WithCookie(cookie.Name.ToString(), cookie.Value.ToString());
            }
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

        public ResponseAssertion ShouldBeHandled(User user = null)
        {
            var response = SendRequest();
            return new ResponseAssertion(response, this);
        }

        public ResponseAssertion ShouldSuccess(User user = null)
        {
            return ShouldBeHandled(user)
                .WithResponse(ResponseAssertion.Is2XxSuccess);
        }

        public ResponseAssertion ShouldSuccessWithRedirect(User user = null)
        {
            return ShouldBeHandled(user)
                .WithResponse(ResponseAssertion.IsSuccessRedirect);
        }
        
        public ResponseAssertion ShouldFail(User user = null)
        {
            return ShouldBeHandled(user)
                .WithResponse(res => !ResponseAssertion.Is2XxSuccess(res) && !ResponseAssertion.IsSuccessRedirect(res));
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

            if (_cookies.Any())
            {
                _cookies.ForEach(c => requestBuilder.WithCookie(c.Name.ToString(), c.Value.ToString()));
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
                return WithResponse(IsSigninRedirect);
            }

            public TestHttpRequestBuilder And { get; }

            public static bool Is2XxSuccess(HttpResponseMessage response)
            {
                var statusCode = (int)response.StatusCode;
                return statusCode >= 200 && statusCode <= 299;
            } 
            
            public static bool IsRedirect(HttpResponseMessage response)
            {
                var statusCode = (int)response.StatusCode;
                return statusCode == 301 || statusCode == 302;
            }
            
            public static bool IsSigninRedirect(HttpResponseMessage response)
            {
                if (!IsRedirect(response))
                {
                    return false;
                }

                var location = response.Headers.Location?.ToString();
                return location != null && location.Contains("signin");
            }
            
            public static bool IsSuccessRedirect(HttpResponseMessage response)
            {
                return IsRedirect(response) && !IsSigninRedirect(response);
            } 
        }
        
        
        
    }
    
    public enum SigninRequirement
    {
        SigninRequired,
        SigninNotRequired
    }
}