using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Discussion.Web.Services.Identity
{
    /// <summary>
    /// Extended UserManager that uses email address when generating confirmation token
    /// </summary>
    /// <remarks>
    /// EmailConfirmationToken generated from Default UserManager does not reflect user's email address
    /// See https://github.com/aspnet/Identity/issues/2062 
    /// </remarks>
    public class EmailAddressAwareUserManager<TUser> : AspNetUserManager<TUser> where TUser : class
    {
        public EmailAddressAwareUserManager(IUserStore<TUser> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher, IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            
        }

        public override async Task<string> GenerateEmailConfirmationTokenAsync(TUser user)
        {
            this.ThrowIfDisposed();
            
            var emailAddress = await this.GetEmailAsync(user);
            return await GenerateUserTokenAsync(user,
                Options.Tokens.EmailConfirmationTokenProvider,
                ComposePurpose(emailAddress));
        }

        public override async Task<IdentityResult> ConfirmEmailAsync(TUser user, string token)
        {
            this.ThrowIfDisposed();
            
            if (user == null)
                throw new ArgumentNullException(nameof (user));

            if (!(this.Store is IUserEmailStore<TUser> emailStore))
                throw new NotSupportedException("StoreNotIUserEmailStore");
            
            var emailAddress = await this.GetEmailAsync(user);

            var verified = await this.VerifyUserTokenAsync(user,
                Options.Tokens.EmailConfirmationTokenProvider,
                ComposePurpose(emailAddress),
                token);
            
            if (!verified)
                return IdentityResult.Failed(this.ErrorDescriber.InvalidToken());
            await emailStore.SetEmailConfirmedAsync(user, true, this.CancellationToken);
            return await this.UpdateUserAsync(user);
        }

        private static string ComposePurpose(string emailAddress)
        {
            var purpose = $"EmailConfirmation;{emailAddress}";
            return purpose;
        }
    }
}