using Discussion.Web.Controllers;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Discussion.Web.Tests.Specs.Models
{
    [Collection("AppSpecs")]
    public class SigninUserModelValidationSpecs
    {
        private readonly Application _myApp;
        public SigninUserModelValidationSpecs(Application app)
        {
            _myApp = app;
        }
        
        [Fact]
        public void should_validate_normal_username_and_password_values_as_valid()
        {
            var ctrl = _myApp.CreateControllerAndValidate<AccountController>(
                new SigninUserViewModel
                {
                    UserName = "validusername",
                    Password = "Mypassword"
                });

            Assert.True(ctrl.ModelState.IsValid);
            ctrl.ModelState.Keys.ShouldNotContain("UserName");
            ctrl.ModelState.Keys.ShouldNotContain("Password");
        }
      
             
        [Theory]
        [InlineData("ausernamelessthan20")]
        [InlineData("userName")]
        [InlineData("valid-user")]
        [InlineData("valid_")]
        [InlineData("00valid")]
        [InlineData("007")]
        public void should_validate_valid_username_values_as_valid(string username)
        {
            var ctrl = _myApp.CreateControllerAndValidate<AccountController>(
                new SigninUserViewModel
                {
                    UserName = username,
                    Password = "password1"
                });

            Assert.True(ctrl.ModelState.IsValid);
            ctrl.ModelState.Keys.ShouldNotContain("UserName");
            ctrl.ModelState.Keys.ShouldNotContain("Password");
        }

        [Theory]
        [InlineData("ausernamelongerthan20")]
        [InlineData("user name")]
        [InlineData("^valid")]
        [InlineData("valid@")]
        [InlineData("va#lid")]
        [InlineData("in")]
        public void should_validate_invalid_username_values_as_invalid(string username)
        {
            var ctrl = _myApp.CreateControllerAndValidate<AccountController>(
                new SigninUserViewModel
                {
                    UserName = username,
                    Password = "password1"
                });

            Assert.False(ctrl.ModelState.IsValid);
            ctrl.ModelState.Keys.ShouldContain("UserName");
            ctrl.ModelState.Keys.ShouldNotContain("Password");
        }
   
        [Theory]
        [InlineData("pass word")]
        [InlineData("thePassword01!")]
        [InlineData("LRo39sCeQU7$")]
        [InlineData("#^%&Nz@&^7asd$")]
        [InlineData("passWord")]
        [InlineData("p@ssw0rd")]
        [InlineData("11111a")]
        [InlineData("a11111")]
        [InlineData("$11113")]
        [InlineData("{11113")]
        [InlineData("11113+")]
        [InlineData("11113=")]
        [InlineData("11113<")]
        [InlineData("11113?")]
        public void should_validate_valid_password_values_as_valid(string password)
        {
            var ctrl = _myApp.CreateControllerAndValidate<AccountController>(
                new SigninUserViewModel
                {
                    UserName = "validuser",
                    Password = password
                });

            Assert.True(ctrl.ModelState.IsValid);
            ctrl.ModelState.Keys.ShouldNotContain("UserName");
            ctrl.ModelState.Keys.ShouldNotContain("Password");
        }
        
        
        
        [Theory]
        [InlineData("Apasswordlongerthan20")]
        [InlineData("password")]
        [InlineData("111111")]
        [InlineData("1111a")]
        [InlineData("F1111")]
        [InlineData("1111G")]
        [InlineData("*&@%~%!")]
        public void should_validate_invalid_password_values_as_invalid(string password)
        {
            var ctrl = _myApp.CreateControllerAndValidate<AccountController>(
                new SigninUserViewModel
                {
                    UserName = "validuser",
                    Password = password
                });

            Assert.False(ctrl.ModelState.IsValid);
            ctrl.ModelState.Keys.ShouldNotContain("UserName");
            ctrl.ModelState.Keys.ShouldContain("Password");
        }

    }
    
}