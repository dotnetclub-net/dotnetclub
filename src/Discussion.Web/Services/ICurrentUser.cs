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
        private readonly HttpContext _httpContext;

        public HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }
        
        public User DiscussionUser => _httpContext.DiscussionUser();
    }
}