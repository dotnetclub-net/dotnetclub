using System;
using System.Linq;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Tests.Common;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    [Collection("WebSpecs")]
    public class UserManagerSpecs
    {
        private readonly IRepository<User> _userRepo;
        private readonly UserManager<User> _userManager;
        private TestDiscussionWebApp _app;

        public UserManagerSpecs(TestDiscussionWebApp app)
        {
            _app = app;
            _userManager = app.GetService<UserManager<User>>();
            _userRepo = app.GetService<IRepository<User>>();
            _app.DeleteAll<User>();
        }

        [Fact]
        public async Task should_confirm_user_email_with_valid_token()
        {
            var user = CreateNewUser();
            var tokenString = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            var confirmResult = await _userManager.ConfirmEmailAsync(user, tokenString);

            _app.ReloadEntity(user);
            Assert.True(confirmResult.Succeeded);
            Assert.True(user.EmailAddressConfirmed);
        }

        [Fact]
        public async Task should_not_confirm_user_email_with_invalid_token()
        {
            var user = CreateNewUser();
            var tokenString = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            var invalidToken = "hello" + tokenString;
            var confirmResult = await _userManager.ConfirmEmailAsync(user, invalidToken);

            _app.ReloadEntity(user);
            VerifyFailedToConfirm(confirmResult, user);
        }

        [Fact]
        public async Task should_not_confirm_user_email_with_token_for_another_user()
        {
            var user1 = CreateNewUser();
            var user2 = CreateNewUser("another@someplace.com");
            var user1TokenString = await _userManager.GenerateEmailConfirmationTokenAsync(user1);
            
            var confirmResult = await _userManager.ConfirmEmailAsync(user2, user1TokenString);

            _app.ReloadEntity(user1, user2);
            VerifyFailedToConfirm(confirmResult, user1);
        }

        [Fact]
        public async Task should_not_confirm_user_email_with_non_matching_email_address()
        {
            var user = CreateNewUser();
            var tokenString = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            user.EmailAddress = "another@mail.com";
            user.EmailAddressConfirmed = false;
            _userRepo.Update(user);
            
            _app.ReloadEntity(user);
            var confirmResult = await _userManager.ConfirmEmailAsync(user, tokenString);

            _app.ReloadEntity(user);
            VerifyFailedToConfirm(confirmResult, user);
        }

        
        private static void VerifyFailedToConfirm(IdentityResult confirmResult, User user)
        {
            var errors = confirmResult.Errors.ToList();
            Assert.False(confirmResult.Succeeded);
            Assert.False(user.EmailAddressConfirmed);
            Assert.Equal("InvalidToken", errors[0].Code);
        }


        private User CreateNewUser(string emailAddress = "someone@someplace.com")
        {
            var user = new User
            {
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                UserName = "NotConfirmedUser",
                DisplayName = "NotConfirmedUser",
                HashedPassword = "some-password",
                EmailAddress = emailAddress,
                EmailAddressConfirmed = false
            };
            _userRepo.Save(user);
            return user;
        }

        // QUESTION: should not confirm with expired token?
    }
}