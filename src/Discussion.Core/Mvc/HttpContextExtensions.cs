using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Core.Mvc
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

            var serviceProvider = httpContext.RequestServices;
            return ToDiscussionUser(httpContext.User,  serviceProvider.GetRequiredService<IRepository<User>>());
        }
        
        public static User ToDiscussionUser(this ClaimsPrincipal claimsPrincipal, IRepository<User> userRepo)
        {
            var userId = ExtractUserId(claimsPrincipal);
            if (userId == null)
            {
                return null;
            }
            
            return userRepo.All()
                .Include(u => u.VerifiedPhoneNumber)
                .Include(u => u.AvatarFile)
                .FirstOrDefault(u => u.Id == userId);
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
