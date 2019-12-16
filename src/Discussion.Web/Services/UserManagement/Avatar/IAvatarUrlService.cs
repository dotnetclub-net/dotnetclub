using Discussion.Core.Models;

namespace Discussion.Web.Services.UserManagement.Avatar
{
    public interface IAvatarUrlService
    {
        string GetAvatarUrl(IAuthor one);
    }
}