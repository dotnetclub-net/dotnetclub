using System.Net.Http;

namespace Discussion.Web.Tests
{
    public static class HttpResponseExtensions
    {
        public static string Content(this HttpResponseMessage response)
        {
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}