using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Discussion.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Web.Services.UserManagement.Avatar
{
    public class UserAvatarService : IUserAvatarService, IDisposable
    {
        private const string DefaultAvatarUrl = "/assets/default-avatar.jpg";
        private readonly IUrlHelper _urlHelper;
        private readonly MD5 _md5;

        public UserAvatarService(IUrlHelper urlHelper)
        {
            this._md5 = MD5.Create();
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
                return _urlHelper.Action("DownloadFile", "Common", new {slug = user.AvatarFile.Slug});
            }

            if (user.EmailAddressConfirmed)
            {
                var hash = Md5Hash(user.EmailAddress);
                return $"https://www.gravatar.com/avatar/{hash}?size=160";
            }

            return DefaultAvatarUrl;
        }
        
        
        private string Md5Hash(string emailAddress)
        {
            var bytes = _md5.ComputeHash(Encoding.ASCII.GetBytes(emailAddress));
            return bytes
                .Select(x => x.ToString("x2"))
                .Aggregate(string.Concat);
        }

        void IDisposable.Dispose()
        {
            _md5.Dispose();
        }
    }
}