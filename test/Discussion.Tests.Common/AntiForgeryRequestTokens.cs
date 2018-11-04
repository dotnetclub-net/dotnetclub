using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Net.Http.Headers;

namespace Discussion.Tests.Common
{

    public class AntiForgeryRequestTokens
    {
        public string VerificationToken { get; private set; }
        public Cookie Cookie { get; private set; }
        
        
        public static AntiForgeryRequestTokens GetFromApplication(TestApplication app)
        {
            var homeResponseTask = app.Server.CreateRequest("/").GetAsync();
            homeResponseTask.ConfigureAwait(false);
            homeResponseTask.Wait();
        
            var homeRes = homeResponseTask.Result;
            if (!homeRes.Headers.TryGetValues(HeaderNames.SetCookie, out var cookies))
            {
                cookies = Enumerable.Empty<string>();
            }
            var antiForgeryCookie = SetCookieHeaderValue.ParseList(cookies.ToList()).FirstOrDefault(cookie => cookie.Name.ToString().StartsWith(".AspNetCore.Antiforgery"));
            if (antiForgeryCookie == null)
            {
                throw new InvalidOperationException("无法从服务器获取 AntiForgery Cookie");
            }
        
            var htmlContent = homeRes.ReadAllContent();
            const string tokenStart = "window.__RequestVerificationToken";
            var tokenHtmlContent = htmlContent.Substring(htmlContent.LastIndexOf(tokenStart));
            var tokenPattern = new Regex(@"^window\.__RequestVerificationToken[^']+'(?<token>[^']+)';");

            var token = tokenPattern.Match(tokenHtmlContent).Groups["token"].Value;
            var reqCookie = new Cookie(antiForgeryCookie.Name.ToString(), antiForgeryCookie.Value.ToString());

            return new AntiForgeryRequestTokens
            {
                VerificationToken = token,
                Cookie = reqCookie
            };
        }
    }
}