using Discussion.Core.Models.UserAvatar;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Services.UserManagement.Avatar.UrlGenerators
{
    public class StorageFileAvatarUrlGenerator : IUserAvatarUrlGenerator<StorageFileAvatar>
    {
        private readonly IUrlHelper _urlHelper;

        public StorageFileAvatarUrlGenerator(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }
        
        public string GetUserAvatarUrl(StorageFileAvatar avatar)
        {
            // ReSharper disable Mvc.ActionNotResolved
            // ReSharper disable Mvc.ControllerNotResolved
            return _urlHelper.Action("DownloadFile", 
                "Common", 
                new {slug = avatar.StorageFileSlug}, 
                _urlHelper.ActionContext.HttpContext.Request.Scheme);
        }
    }
}