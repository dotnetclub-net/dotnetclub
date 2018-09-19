using Discussion.Web.Models;
using Microsoft.AspNetCore.Http;

namespace Discussion.Web.Services.Identity
{
    public static class PrincipalContext
    {

        public static bool IsAuthenticated(this HttpContext httpContext)
        {
            var isAuthedExpr = httpContext?.User?.Identity?.IsAuthenticated;
            return isAuthedExpr.HasValue && isAuthedExpr.Value; 
        }
        
        public static DiscussionPrincipal DiscussionUser(this HttpContext httpContext)
        {
            if (!IsAuthenticated(httpContext))
            {
                return null;
            }

            return httpContext.User as DiscussionPrincipal;
        }



    }
}
