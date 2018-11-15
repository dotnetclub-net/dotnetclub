using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Discussion.Core.Models;
using Microsoft.AspNetCore.Mvc;


namespace Discussion.Web.Services.Impl
{
    public class UserAvatarService : IUserAvatarService
    {
        private const string DefaultAvatarUrl = "/assets/default-avatar.jpg";
        private readonly IUrlHelper _urlHelper;

        public UserAvatarService(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }
        
        
        public string GetUserAvatarUrl(User user)
        {
            if (user == null)
            {
                return null;
            }

            if (user.AvatarFileId > 0)
            {
                // ReSharper disable Mvc.ActionNotResolved
                // ReSharper disable Mvc.ControllerNotResolved
                return _urlHelper.Action("DownloadFile", "Common", new {id = user.AvatarFileId});
            }

            if (user.EmailAddressConfirmed)
            {
                var hash = Md5Hash(user.EmailAddress);
                return $"https://www.gravatar.com/avatar/{hash}?size=160";
            }

            return DefaultAvatarUrl;
        }
        
        
        private static string Md5Hash(string emailAddress)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(Encoding.ASCII.GetBytes(emailAddress));
                return bytes
                    .Select(x => x.ToString("x2"))
                    .Aggregate(string.Concat);
            }
        }
    }
}