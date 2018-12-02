using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Discussion.Web.Services.UserManagement.Identity
{
    public class EmailConfirmationTokenOptions: DataProtectionTokenProviderOptions
    {
        
    }
    
    
    public class EmailConfirmationTokenProvider<TUser>: DataProtectorTokenProvider<TUser> where TUser : class
    {
        public EmailConfirmationTokenProvider(
            IDataProtectionProvider dataProtectionProvider, 
            IOptions<EmailConfirmationTokenOptions> options) 
            : base(dataProtectionProvider, options)
        {
        }
    }


}