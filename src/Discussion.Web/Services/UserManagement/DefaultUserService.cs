using System.Linq;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Discussion.Web.Services.UserManagement
{
    public class DefaultUserService : IUserService
    {
        private readonly IRepository<User> _userRepo;
        private readonly UserManager<User> _userManager;

        public DefaultUserService(IRepository<User> userRepo, UserManager<User> userManager)
        {
            _userRepo = userRepo;
            _userManager = userManager;
        }

        public async Task<IdentityResult> UpdateUserInfo(UserSettingsViewModel userSettingsViewModel, User user)
        {
            var updateEmailResult = await UpdateEmail(userSettingsViewModel, user);
            if (!updateEmailResult.Succeeded)
            {
                return updateEmailResult;
            }
            
            user.AvatarFileId = userSettingsViewModel.AvatarFileId;
            user.DisplayName = userSettingsViewModel.DisplayName;
            if (string.IsNullOrWhiteSpace(user.DisplayName))
            {
                user.DisplayName = user.UserName;
            }

            _userRepo.Update(user);
            return IdentityResult.Success;
        }

        private async Task<IdentityResult> UpdateEmail(UserSettingsViewModel userSettingsViewModel, User user)
        {
            var existingEmail = user.EmailAddress?.Trim();
            var newEmail = userSettingsViewModel.EmailAddress?.Trim();
            var emailNotChanged = existingEmail.IgnoreCaseEqual(newEmail);
            if (emailNotChanged)
            {
                return IdentityResult.Success;
            }
            
            var emailTaken = IsEmailTakenOtherUser(user.Id, newEmail);
            if (emailTaken)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "EmailTaken",
                    Description = "邮件地址已由其他用户使用"
                });
            }
            return await _userManager.SetEmailAsync(user, newEmail);
        }
        
        
        public bool IsEmailTakenOtherUser(int thisUserId, string checkingEmail)
        {
            if (string.IsNullOrWhiteSpace(checkingEmail))
            {
                return false;
            }
            
            return _userRepo.All()
                .Any(u => u.EmailAddressConfirmed
                          && u.Id != thisUserId
                          && u.EmailAddress != null
                          && u.EmailAddress.ToLower() == checkingEmail.ToLower());
        }

    }
}