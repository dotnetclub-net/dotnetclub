using Discussion.Core.Models.UserAvatar;

namespace Discussion.Web.Services.UserManagement.Avatar.UrlGenerators
{
    public class DefaultAvatarUrlGenerator : IUserAvatarUrlGenerator<DefaultAvatar>
    {
        public string GetUserAvatarUrl(DefaultAvatar avatar)
        {
            return "/assets/default-avatar.jpg";
        }
    }
}