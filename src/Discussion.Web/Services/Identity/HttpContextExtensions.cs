using System.Linq;
using System.Security.Claims;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Web.Services.Identity
{
    public static class HttpContextExtensions
    {
        public static bool IsAuthenticated(this HttpContext httpContext)
        {
            return IsAuthenticated(httpContext?.User); 
        }
        
        public static bool IsAuthenticated(this ClaimsPrincipal claimsPrincipal)
        {
            var isAuthedExpr = claimsPrincipal?.Identity?.IsAuthenticated;
            return isAuthedExpr.HasValue && isAuthedExpr.Value; 
        }
        
        public static User DiscussionUser(this HttpContext httpContext)
        {
            if (!IsAuthenticated(httpContext))
            {
                return null;
            }

            return ToDiscussionUser(httpContext.User,  httpContext.RequestServices.GetRequiredService<IRepository<User>>());
        }
        
        public static User ToDiscussionUser(this ClaimsPrincipal claimsPrincipal, IRepository<User> userRepo)
        {
            var userId = ExtractUserId(claimsPrincipal);
            return userId == null ? null : userRepo.Get(userId.Value);
        }

        public static int? ExtractUserId(this ClaimsPrincipal claimsPrincipal)
        {
            bool IsIdClaim(Claim claim)
            {
                return claim.Type == ClaimTypes.NameIdentifier;
            }

            var identity = claimsPrincipal.Identities.FirstOrDefault(id => id.HasClaim(IsIdClaim));
            var userIdClaim = identity?.Claims.FirstOrDefault(IsIdClaim)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return userId;
        }
    }
}
