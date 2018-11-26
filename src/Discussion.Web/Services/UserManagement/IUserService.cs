using System.Threading.Tasks;
using Discussion.Core.Models;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Discussion.Web.Services.UserManagement
{
    public interface IUserService
    {
        Task<IdentityResult> UpdateUserInfoAsync(User user, UserSettingsViewModel userSettingsViewModel);

        Task SendEmailConfirmationMailAsync(User user, string urlProtocol);

        Task<IdentityResult> ConfirmEmailAsync(User user, UserEmailToken tokenInEmail);

        Task SendPhoneNumberVerificationCodeAsync(User user, string phoneNumber);
        
        void VerifyPhoneNumberByCode(User user, string verificationCode);

    }
}