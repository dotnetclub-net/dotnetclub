using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Discussion.Core.Middleware
{
    public class RejectRevokedSessionMiddleware
    {
        public static List<string> RevokedTokens = new List<string>(); 
        private readonly RequestDelegate _next;

        public RejectRevokedSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task Invoke(HttpContext ctx)
        {
//            var readonlyDataSettings = ctx.RequestServices.GetService<IReadonlyDataSettings>() as ReadonlyDataSettings;

            var user = ctx.User;
            var sessionId = user?.Claims.FirstOrDefault(c => c.Type == "SessionId")?.Value;
            if (sessionId != null && RevokedTokens.Contains(sessionId))
            {
                await ctx.SignOutAsync(IdentityConstants.ApplicationScheme);
                await ctx.SignOutAsync(IdentityConstants.ExternalScheme);
                ctx.User = null;
            }

            await _next(ctx);
        }
    }
}