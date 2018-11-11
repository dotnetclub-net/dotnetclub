using System;
using System.Linq;
using System.Net;
using Discussion.Admin.Controllers;
using Discussion.Admin.Services;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Utilities;
using Discussion.Core.ViewModels;
using Discussion.Tests.Common;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Discussion.Admin.Tests.Specs.Controllers
{
    [Collection("AdminSpecs")]
    public class AccountControllerSpecs
    {
        private readonly TestDiscussionAdminApp _adminApp;
        private readonly IRepository<AdminUser> _adminUserRepo;

        public AccountControllerSpecs(TestDiscussionAdminApp adminApp)
        {
            _adminApp = adminApp;
            _adminUserRepo = adminApp.GetService<IRepository<AdminUser>>();
            _adminApp.Reset();
            _adminApp.DeleteAll<AdminUser>();
        }

        [Fact]
        public void should_register_first_admin_without_signin()
        {
            var newAdminUser = new UserViewModel
            {
                UserName = StringUtility.Random(),
                Password = "abcdefA@"
            };
            
            var accountController = _adminApp.CreateController<AccountController>();
            var apiResponse = accountController.Register(newAdminUser);
            
            
            var found = _adminUserRepo.All().FirstOrDefault(u => u.Username == newAdminUser.UserName);
            Assert.Equal(200, apiResponse.Code);
            Assert.NotNull(found);
            Assert.Equal(newAdminUser.UserName, found.Username);
            Assert.NotNull(found.HashedPassword);
            Assert.NotEqual(newAdminUser.Password, found.HashedPassword);
        }

        [Fact]
        public void should_register_new_admin_after_signin()
        {
            _adminApp.MockAdminUser();
            var newAdminUser = new UserViewModel
            {
                UserName = StringUtility.Random(),
                Password = "abcdefA@"
            };
            
            var accountController = _adminApp.CreateController<AccountController>();
            var apiResponse = accountController.Register(newAdminUser);
            
            
            var found = _adminUserRepo.All().Any(u => u.Username == newAdminUser.UserName);
            Assert.Equal(200, apiResponse.Code);
            Assert.True(found);
        }

        [Fact]
        public void should_not_register_second_admin_without_signin()
        {
            _adminUserRepo.Save(new AdminUser
            {
                Username = "first-admin"
            });

            var newAdminUser = new UserViewModel
            {
                UserName = StringUtility.Random(),
                Password = "abcdefA@"
            };

            
            var accountController = _adminApp.CreateController<AccountController>();
            var apiResponse = accountController.Register(newAdminUser);
            
            
            var found = _adminUserRepo.All().Any(u => u.Username == newAdminUser.UserName);
            Assert.Equal(401, apiResponse.Code);
            Assert.False(found);
        }

        [Fact]
        public void should_not_register_one_username_a_second_time()
        {
            var password = "12345A";
            var hashedPassword = _adminApp.GetService<IAdminUserService>().HashPassword(password);
            var firstAdminUser = new AdminUser { Username = "first-admin", HashedPassword = hashedPassword  };
            _adminUserRepo.Save(firstAdminUser);
            _adminApp.MockAdminUser();
            
            
            var newAdminUser = new UserViewModel
            {
                UserName = firstAdminUser.Username,
                Password = "another-password"
            };
            var accountController = _adminApp.CreateController<AccountController>();
            var apiResponse = accountController.Register(newAdminUser);
            
            
            var users = _adminUserRepo.All().Where(u => u.Username == newAdminUser.UserName).ToList();
            Assert.Equal(400, apiResponse.Code);
            Assert.Equal(1, users.Count);
        }
        
        [Theory]
        [InlineData("bad-password", "111111")]
        [InlineData("bad-password", "11111")]
        [InlineData("bad-password", "aaaaaa")]
        [InlineData("bad-password", "AAABBB")]
        [InlineData("ab中文", "bad-username")]
        [InlineData("jijie.chen", "bad-username")]
        [InlineData("some@here", "bad-username")]
        public void should_not_register_admin_with_invalid_username_or_password(string username, string password)
        {
            var newAdminUser = new UserViewModel
            {
                UserName = username,
                Password = password
            };

            
            var accountController = _adminApp.CreateController<AccountController>();
            accountController.TryValidateModel(newAdminUser);
            var apiResponse = accountController.Register(newAdminUser);
            
            
            var found = _adminUserRepo.All().Any(u => u.Username == newAdminUser.UserName);
            Assert.Equal(400, apiResponse.Code);
            Assert.True(!string.IsNullOrEmpty(apiResponse.ErrorMessage));
            Assert.False(found);
        }
        
        [Fact]
        public void should_signin_admin_with_valid_credential()
        {
            var newAdminUser = new UserViewModel
            {
                UserName = StringUtility.Random(),
                Password = "abcdefA@"
            };
            
            var accountController = _adminApp.CreateController<AccountController>();
            accountController.Register(newAdminUser);
            var found = _adminUserRepo.All().FirstOrDefault(u => u.Username == newAdminUser.UserName);

            dynamic signinResult = accountController.Signin(newAdminUser);
            
            int userId = signinResult.Id;
            string jwtToken = signinResult.Token;
            int expires = signinResult.ExpiresInSeconds;

            Assert.Equal(found.Id, userId);
            Assert.Equal(TimeSpan.FromMinutes(120).TotalSeconds, expires);
            Assert.NotNull(jwtToken);
        }
        
        [Fact]
        public void should_not_signin_admin_with_non_existing_username()
        {
            var newAdminUser = new UserViewModel
            {
                UserName = "someone",
                Password = "abcdefA@"
            };
            
            var signinResult = _adminApp.CreateController<AccountController>()
                                        .Signin(newAdminUser) as ApiResponse;
           
            Assert.Equal((int)HttpStatusCode.BadRequest, signinResult.Code);
            Assert.False(signinResult.HasSucceeded);
        }
        
        [Fact]
        public void should_not_signin_admin_with_wrong_password()
        {
            var newAdminUser = new UserViewModel
            {
                UserName = StringUtility.Random(),
                Password = "abcdefA@"
            };
            
            var accountController = _adminApp.CreateController<AccountController>();
            accountController.Register(newAdminUser);

            newAdminUser.Password = "1242kd!";
            var signinResult = _adminApp.CreateController<AccountController>()
                                        .Signin(newAdminUser) as ApiResponse;
           
            Assert.Equal((int)HttpStatusCode.BadRequest, signinResult.Code);
            Assert.False(signinResult.HasSucceeded);
        }

    }
}