using System.Threading.Tasks;
using Discussion.Core.Models;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Discussion.Web.Services.UserManagement
{
    public interface IUserService
    {
        Task<IdentityResult> UpdateUserInfo(UserSettingsViewModel userSettingsViewModel, User user);

        Task SendEmailConfirmationMail(User user, string urlProtocol);
        
        bool IsEmailTakenOtherUser(int thisUserId, string checkingEmail);
    }
}