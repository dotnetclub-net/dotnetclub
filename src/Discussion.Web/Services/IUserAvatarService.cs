using Discussion.Core.Models;

namespace Discussion.Web.Services
{
    public interface IUserAvatarService
    {
        string GetUserAvatarUrl(User user);
    }
}