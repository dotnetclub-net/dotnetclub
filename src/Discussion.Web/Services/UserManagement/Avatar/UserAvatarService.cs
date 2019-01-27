using System;
using Discussion.Core.Models;
using Discussion.Web.Services.UserManagement.Avatar.UrlGenerators;

namespace Discussion.Web.Services.UserManagement.Avatar
{
    public class UserAvatarService : IUserAvatarService
    {
        private readonly IServiceProvider _services;

        public UserAvatarService(IServiceProvider services)
        {
            _services = services;
        }
        
        public string GetUserAvatarUrl(User user)
        {
            var avatar = user.GetAvatar();
            var generatorType = typeof(IUserAvatarUrlGenerator<>).MakeGenericType(avatar.GetType());
            
            var generator = _services.GetService(generatorType);
            var generateMethod = generator.GetType().GetMethod(nameof(DefaultAvatarUrlGenerator.GetUserAvatarUrl));
            return (string) generateMethod.Invoke(generator, new object[]{ user.GetAvatar() });
        }
    }
}