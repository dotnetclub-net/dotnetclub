using System.Linq;
using Discussion.Admin.Controllers;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
using Discussion.Core.ViewModels;
using Discussion.Tests.Common;
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
            accountController.ObjectValidator.Validate(
                accountController.ControllerContext,
                null,
                null,
                newAdminUser);
            var apiResponse = accountController.Register(newAdminUser);
            
            
            var found = _adminUserRepo.All().Any(u => u.Username == newAdminUser.UserName);
            Assert.Equal(400, apiResponse.Code);
            Assert.True(!string.IsNullOrEmpty(apiResponse.ErrorMessage));
            Assert.False(found);
        }
        
        
    }
}