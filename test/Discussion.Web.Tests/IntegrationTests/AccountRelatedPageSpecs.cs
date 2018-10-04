using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("AppSpecs")]
    public class AccountRelatedPageSpecs
    {
        private readonly TestApplication _theApp;
        private readonly AntiForgeryRequestTokens _antiForgeryTokens;

        public AccountRelatedPageSpecs(TestApplication theApp) {
            _theApp = theApp.Reset();
            _antiForgeryTokens = _theApp.GetAntiForgeryTokens();
        }


        [Fact]
        public async Task should_serve_signin_page_correctly()
        {
            // arrange
            var request = _theApp.Server.CreateRequest("/signin");

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.ReadAllContent().ShouldContain("用户登录");            
        }
        
          
        [Fact]
        public async Task should_be_able_to_signin_new_user()
        {
            // arrange
            var username = Guid.NewGuid().ToString("N").Substring(4, 8);
            var password = "11111a";
            _theApp.CreateUser(username, password);

            // Act
            var request = _theApp.Server.CreateRequest("/signin")
                .WithFormContent(new Dictionary<string, string>()
                {
                    {"UserName", username}, 
                    {"Password", password},
                    {"__RequestVerificationToken", _antiForgeryTokens.VerificationToken}
                })
                .WithCookie(_antiForgeryTokens.Cookie);
            var response = await request.PostAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.Redirect);
            var cookieHeaders = response.Headers.GetValues("Set-Cookie").ToList();
            cookieHeaders.ShouldContain(cookie => cookie.Contains(".AspNetCore.Identity.Application"));
        }
        
        [Fact]
        public async Task signed_in_users_should_be_able_to_view_pages_that_requires_authenticated_users()
        {
            // arrange
            var username = Guid.NewGuid().ToString("N").Substring(4, 8);
            var password = "11111a";
            _theApp.CreateUser(username, password);
            var signinResponse = await _theApp.RequestAntiForgeryForm("/signin",
                                                new Dictionary<string, string>
                                                {
                                                    {"UserName", username},
                                                    {"Password", password}
                                                })
                                                .PostAsync();
            // Act
            var request = _theApp.Server.CreateRequest("/topics/create")
                                        .WithCookiesFrom(signinResponse);
            
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.ReadAllContent().ShouldContain("退出登录");
        }

        [Fact]
        public async Task should_serve_register_page_correctly()
        {
            // arrange
            var request = _theApp.Server.CreateRequest("/register");

            // act
            var response = await request.GetAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.ReadAllContent().ShouldContain("用户注册");            
        }
  
        [Fact]
        public async Task should_be_able_to_register_new_user()
        {
            // arrange
            var username = Guid.NewGuid().ToString("N").Substring(4, 8);
            var password = "11111a";
            
            // Act
            var request = _theApp.Server.CreateRequest("/register")
                .WithFormContent(new Dictionary<string, string>()
                {
                    {"UserName", username},
                    {"Password", password},
                    {"__RequestVerificationToken", _antiForgeryTokens.VerificationToken}
                })
                .WithCookie(_antiForgeryTokens.Cookie);
            var response = await request.PostAsync();

            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.Redirect);
            var isRegistered = _theApp.GetService<IRepository<User>>().All().Any(u => u.UserName == username);
            isRegistered.ShouldEqual(true);
        }
     
        [Fact]
        public async Task should_signin_newly_registered_user()
        {
            // arrange
            var username = Guid.NewGuid().ToString("N").Substring(4, 8);
            var registerResponse = await _theApp.Server.CreateRequest("/register")
                                .WithFormContent(new Dictionary<string, string>()
                                    {
                                        {"UserName", username}, 
                                        {"Password", "11111a"},
                                        {"__RequestVerificationToken", _antiForgeryTokens.VerificationToken}
                                    })
                                .WithCookie(_antiForgeryTokens.Cookie)
                                .PostAsync();

            // Act
            var response = await _theApp.Server.CreateRequest("/topics/create")
                                                .WithCookiesFrom(registerResponse)
                                                .GetAsync();
            
            // assert
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.ReadAllContent().ShouldContain("退出登录");
        }


    }
}