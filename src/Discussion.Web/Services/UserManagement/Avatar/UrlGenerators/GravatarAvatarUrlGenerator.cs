using System;
using System.Linq;
using System.Text;
using Discussion.Core.Models.UserAvatar;
using MimeKit.Cryptography;

namespace Discussion.Web.Services.UserManagement.Avatar.UrlGenerators
{
    public class GravatarAvatarUrlGenerator : IUserAvatarUrlGenerator<GravatarAvatar>, IDisposable
    {
        private readonly MD5 _md5;

        public GravatarAvatarUrlGenerator()
        {
            this._md5 = MD5.Create();
        }

        
        public string GetUserAvatarUrl(GravatarAvatar avatar)
        {
            var hash = Md5Hash(avatar.EmailAddress);
            return $"https://www.gravatar.com/avatar/{hash}?size=160";
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