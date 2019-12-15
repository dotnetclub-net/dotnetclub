using System;
using Discussion.Core.Models;
using Discussion.Web.Services.UserManagement.Avatar.UrlGenerators;

namespace Discussion.Web.Services.UserManagement.Avatar
{
    public class DispatchAvatarUrlService : IAvatarUrlService
    {
        private readonly IServiceProvider _services;

        public DispatchAvatarUrlService(IServiceProvider services)
        {
            _services = services;
        }
        
        public string GetAvatarUrl(IAuthor one)
        {
            var avatar = one.GetAvatar();
            var generatorType = typeof(IUserAvatarUrlGenerator<>).MakeGenericType(avatar.GetType());
            
            var generator = _services.GetService(generatorType);
            var generateMethod = generator.GetType().GetMethod(nameof(DefaultAvatarUrlGenerator.GetUserAvatarUrl));
            return (string) generateMethod.Invoke(generator, new object[]{ one.GetAvatar() });
        }

        public string GetTopics(int page)
        {
            throw new NotImplementedException();
        }

        public string GetReplies(int page)
        {
            throw new NotImplementedException(); 
        }
    }
}