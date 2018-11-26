using Discussion.Core.Models;

namespace Discussion.Web.Services.UserManagement.Avatar
{
    public interface IUserAvatarService
    {
        string GetUserAvatarUrl(User user);
    }
}