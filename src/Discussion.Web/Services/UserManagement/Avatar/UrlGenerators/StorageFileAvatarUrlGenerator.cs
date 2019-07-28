using Discussion.Core.Models.UserAvatar;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Services.UserManagement.Avatar.UrlGenerators
{
    public class StorageFileAvatarUrlGenerator : IUserAvatarUrlGenerator<StorageFileAvatar>
    {
        private readonly IUrlHelper _urlHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StorageFileAvatarUrlGenerator(IUrlHelper urlHelper, IHttpContextAccessor httpContextAccessor)
        {
            _urlHelper = urlHelper;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public string GetUserAvatarUrl(StorageFileAvatar avatar)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            // ReSharper disable Mvc.ActionNotResolved
            // ReSharper disable Mvc.ControllerNotResolved
            return _urlHelper.Action("DownloadFile", 
                "Common", 
                new {slug = avatar.StorageFileSlug}, 
                request.Scheme, 
                request.Host.ToString());
        }
    }
}