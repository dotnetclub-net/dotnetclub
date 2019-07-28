using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Discussion.Core.Models;
using Discussion.Core.Models.UserAvatar;
using Discussion.Web.Services.UserManagement.Avatar;
using Discussion.Web.Services.UserManagement.Avatar.UrlGenerators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    public class UserAvatarServiceSpecs
    {
        [Fact]
        public void should_generate_default_avatar()
        {
            var user = new User();

            var services = CreateServices(CreateMockUrlHelper());
            var avatarUrl = services.GetService<IAvatarUrlService>().GetAvatarUrl(user);
            
            Assert.Equal("/assets/default-avatar.jpg", avatarUrl);
        }
        
        [Fact]
        public void should_generate_avatar_from_user_set_avatar()
        {
            var user = new User {AvatarFileId = 12, AvatarFile = new FileRecord() {Id = 12, Slug = "file-hash"}};

            var services = CreateServices(CreateMockUrlHelper(user.AvatarFile.Slug));
            var avatarUrl = services.GetService<IAvatarUrlService>().GetAvatarUrl(user);
            
            Assert.Equal("http://download/file-hash", avatarUrl);
        }
        
        [Fact]
        public void should_generate_by_confirmed_email_address_avatar()
        {
            var user = new User
            {
                EmailAddressConfirmed = true, 
                EmailAddress = "someone@someplace.com"
            };

            var services = CreateServices(CreateMockUrlHelper());
            var avatarUrl = services.GetService<IAvatarUrlService>().GetAvatarUrl(user);

            var hash = Md5Hash(user.EmailAddress);
            Assert.Equal($"https://www.gravatar.com/avatar/{hash}?size=160&d=robohash", avatarUrl);
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


        private static IUrlHelper CreateMockUrlHelper(string slug = null)
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>()))
                .Returns("http://download/" + (slug ?? "file"));
            urlHelper.Setup(s => s.ActionContext)
                .Returns(new ActionContext() {HttpContext = new DefaultHttpContext()});
            return urlHelper.Object;
        }

        private static IServiceProvider CreateServices(IUrlHelper urlHelper)
        {
            var services = new ServiceCollection();
            
            services.AddScoped(sp => urlHelper);
            services.AddSingleton<IAvatarUrlService, DispatchAvatarUrlService>();
            services.AddScoped<IUserAvatarUrlGenerator<DefaultAvatar>, DefaultAvatarUrlGenerator>();
            services.AddScoped<IUserAvatarUrlGenerator<StorageFileAvatar>, StorageFileAvatarUrlGenerator>();
            services.AddScoped<IUserAvatarUrlGenerator<GravatarAvatar>, GravatarAvatarUrlGenerator>();

            return services.BuildServiceProvider();

        }

        
        
    }
}