using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Discussion.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace Discussion.Web.Services.Identity
{
    public class DiscussionUserClaimsPrincipalFactory: IUserClaimsPrincipalFactory<User>
    {
        public Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.Integer32),
                new Claim(ClaimTypes.Name, user.DisplayName, ClaimValueTypes.String),
                new Claim("SigninTime", System.DateTime.UtcNow.Ticks.ToString(), ClaimValueTypes.Integer64)
            };

            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            return Task.FromResult(new ClaimsPrincipal(identity));
        }
    }
}