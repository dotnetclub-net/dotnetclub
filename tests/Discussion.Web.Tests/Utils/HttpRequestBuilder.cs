using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Discussion.Web.Tests.Utils
{
    public class HttpRequestBuilder
    {

        public HttpRequestBuilder WithPath(string path)
        {
            return this;
        }

        public HttpRequestBuilder WithQuery(string query)
        {
            return this;
        }


        public HttpRequestBuilder WithHttpMethod(string httpMethod)
        {
            return this;
        }

        public HttpRequestBuilder WithHeader(string name, string value)
        {
            return this;
        }

        public HttpRequestBuilder WithCookie(string name, string value)
        {
            return this;
        }


        public HttpRequestBuilder WithCookies(IDictionary<string, string> cookies)
        {
            return this;
        }


        public HttpRequestBuilder WithFile(string filePath)
        {
            return this;
        }

        public HttpRequestBuilder WithFile(Stream fileContent)
        {
            return this;
        }


        public HttpRequestBuilder WithContent(string content)
        {
            return this;
        }

        public HttpRequestBuilder WithContent(Stream uploadContent)
        {
            return this;
        }

        public HttpRequestBuilder WithContentType(string name, string value)
        {
            return this;
        }

        public HttpRequestBuilder WithWebForm(string name, string value)
        {
            return this;
        }


        public HttpContext SendRequest()
        {
            throw new NotImplementedException();
        }


    }
}
