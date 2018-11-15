using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Discussion.Core.Models;
using Discussion.Web.Services.Impl;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    [Collection("WebSpecs")]
    public class UserAvatarServiceSpecs
    {

        [Fact]
        public void should_generate_default_avatar()
        {
            var user = new User();

            var userAvatarService = new UserAvatarService(CreateMockUrlHelper());
            var avatarUrl = userAvatarService.GetUserAvatarUrl(user);
            
            Assert.Equal("/assets/default-avatar.jpg", avatarUrl);
        }
        
        [Fact]
        public void should_generate_avatar_from_user_set_avatar()
        {
            var user = new User();
            user.AvatarFileId = 12;

            var userAvatarService = new UserAvatarService(CreateMockUrlHelper(user.AvatarFileId));
            var avatarUrl = userAvatarService.GetUserAvatarUrl(user);
            
            Assert.Equal("http://download/12", avatarUrl);
        }
        
        [Fact]
        public void should_generate_by_confirmed_email_address_avatar()
        {
            var user = new User
            {
                EmailAddressConfirmed = true, 
                EmailAddress = "someone@someplace.com"
            };

            var userAvatarService = new UserAvatarService(CreateMockUrlHelper());
            var avatarUrl = userAvatarService.GetUserAvatarUrl(user);

            var hash = Md5Hash(user.EmailAddress);
            Assert.Equal($"https://www.gravatar.com/avatar/{hash}?size=160", avatarUrl);
        }

        private string Md5Hash(string emailAddress)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(Encoding.ASCII.GetBytes(emailAddress));
                return bytes
                    .Select(x => x.ToString("x2"))
                    .Aggregate(string.Concat);
            }
        }


        private static IUrlHelper CreateMockUrlHelper(int? fileId = null)
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>()))
                .Returns("http://download/" + fileId ?? "0");
            return urlHelper.Object;
        }

        
        
    }
}