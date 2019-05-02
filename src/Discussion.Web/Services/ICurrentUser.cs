using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Microsoft.AspNetCore.Http;

namespace Discussion.Web.Services
{
    public interface ICurrentUser
    {
        User DiscussionUser { get; }
    }

    public class HttpContextCurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public User DiscussionUser => _httpContextAccessor.HttpContext.DiscussionUser();
    }
}