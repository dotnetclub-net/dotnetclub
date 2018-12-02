using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Discussion.Core.Models;
using Discussion.Core.Time;
using Microsoft.AspNetCore.Identity;

namespace Discussion.Web.Services.UserManagement.Identity
{
    public class DiscussionUserClaimsPrincipalFactory: IUserClaimsPrincipalFactory<User>
    {
        private readonly IClock _clock;
        public DiscussionUserClaimsPrincipalFactory(IClock clock)
        {
            _clock = clock;
        }
        
        public Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.Integer32),
                new Claim(ClaimTypes.Name, user.DisplayName, ClaimValueTypes.String),
                new Claim("SigninTime", _clock.Now.UtcTicks.ToString(), ClaimValueTypes.Integer64)
            };

            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            return Task.FromResult(new ClaimsPrincipal(identity));
        }
    }
}