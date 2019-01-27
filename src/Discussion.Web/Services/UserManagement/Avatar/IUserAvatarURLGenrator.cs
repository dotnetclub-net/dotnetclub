using Discussion.Core.Models.UserAvatar;

namespace Discussion.Web.Services.UserManagement.Avatar
{
    public interface IUserAvatarUrlGenerator<in TAvatar> where TAvatar: IUserAvatar
    {
        string GetUserAvatarUrl(TAvatar avatar);
    }
}