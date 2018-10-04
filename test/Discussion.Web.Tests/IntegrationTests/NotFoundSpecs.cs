using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Xunit;


namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("AppSpecs")]
    public class NotFoundSpecs
    {
        public const string NotFoundPath = "/something-not-defined";
        public const string NotFoundStaticFile = "/something-not-defined.css";

        private readonly TestApplication _theApp;
        public NotFoundSpecs(TestApplication theApp)
        {
            _theApp = theApp;
        }

        [Fact]
        public async Task should_response_not_found_by_default()
        {
            // act
            var response = await _theApp.Server.CreateRequest(NotFoundPath).GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task should_response_not_found_for_a_static_file_path()
        {
            // act
            var response = await _theApp.Server.CreateRequest(NotFoundStaticFile).GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.NotFound);
        }
           
        [Fact]
        public async Task should_reject_post_request_without_valid_anti_forgery_token()
        {
            // arrange
            var username = Guid.NewGuid().ToString("N").Substring(4, 8);
            var password = "11111a";
            var tokens = _theApp.GetAntiForgeryTokens();
            
            // Act
            var request = _theApp.Server.CreateRequest("/register")
                .WithFormContent(new Dictionary<string, string>()
                {
                    {"UserName", username},
                    {"Password", password},
                    {"__RequestVerificationToken", "some invalid token"}
                })
                .WithCookie(tokens.Cookie);
            var response = await request.PostAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            var isRegistered = _theApp.GetService<IRepository<User>>().All().Any(u => u.UserName == username);
            isRegistered.ShouldEqual(false);
        }
        
        [Fact]
        public async Task should_reject_post_request_without_valid_anti_forgery_cookie()
        {
            // arrange
            var username = Guid.NewGuid().ToString("N").Substring(4, 8);
            var password = "11111a";
            var tokens = _theApp.GetAntiForgeryTokens();
            
            // Act
            var request = _theApp.Server.CreateRequest("/register")
                .WithFormContent(new Dictionary<string, string>()
                {
                    {"UserName", username},
                    {"Password", password},
                    {"__RequestVerificationToken", tokens.VerificationToken}
                });
            var response = await request.PostAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.BadRequest);
            var isRegistered = _theApp.GetService<IRepository<User>>().All().Any(u => u.UserName == username);
            isRegistered.ShouldEqual(false);
        }

    }
}
