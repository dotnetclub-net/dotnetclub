using System.Linq;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Core.Middleware
{
    public class RejectRevokedSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public RejectRevokedSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task Invoke(HttpContext ctx)
        {
            var revokedSessionRepo = ctx.RequestServices.GetService<IRepository<SessionRevocationRecord>>();
          
            var user = ctx.User;
            var sessionId = user?.Claims.FirstOrDefault(c => c.Type == "SessionId")?.Value;
            if (sessionId != null && IsSessionRevoked(revokedSessionRepo, sessionId, out var revocationRecord))
            {
                await ctx.SignOutAsync(IdentityConstants.ApplicationScheme);
                await ctx.SignOutAsync("OpenIdConnect");  //  OpenIdConnectDefaults.AuthenticationScheme
                ctx.User = null;
                
                revokedSessionRepo.Delete(revocationRecord);
            }

            await _next(ctx);
        }

        private static bool IsSessionRevoked(IRepository<SessionRevocationRecord> revokedSessionRepo, string sessionId, out SessionRevocationRecord revocationRevocationRecord)
        {
            revocationRevocationRecord = revokedSessionRepo.All().FirstOrDefault(s => s.SessionId == sessionId);
            return revocationRevocationRecord != null;
        }
    }
}