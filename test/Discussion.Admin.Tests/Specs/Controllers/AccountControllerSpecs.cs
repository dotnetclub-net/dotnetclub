using System.Linq;
using Discussion.Admin.Controllers;
using Discussion.Admin.ViewModels;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
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
        }

        [Fact]
        public void should_register_first_admin()
        {
            var newAdminUser = new AdminUserRegistration
            {
                Username = StringUtility.Random(),
                Password = "abcdefA@"
            };

            var accountController = _adminApp.CreateController<AccountController>();
            var apiResponse = accountController.Register(newAdminUser);
            
            Assert.Equal(200, apiResponse.Code);

            var found = _adminUserRepo.All().Any(u => u.Username == newAdminUser.Username);
            Assert.True(found);
        }
    }
}