using Microsoft.AspNetCore.Identity;

namespace Discussion.Web.Resources
{
    public class ResourceBasedIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError()
        {
            var error = base.DefaultError();
            error.Description = IdentityResource.DefaultError;
            return error;
        }

        public override IdentityError ConcurrencyFailure()
        {
            var error = base.ConcurrencyFailure();
            error.Description = IdentityResource.ConcurrencyFailure;
            return error;
        }

        public override IdentityError PasswordMismatch()
        {
            var error = base.PasswordMismatch();
            error.Description = IdentityResource.PasswordMismatch;
            return error;
        }

        public override IdentityError InvalidToken()
        {
            var error = base.InvalidToken();
            error.Description = IdentityResource.InvalidToken;
            return error;
        }

        public override IdentityError RecoveryCodeRedemptionFailed()
        {
            var error = base.RecoveryCodeRedemptionFailed();
            error.Description = IdentityResource.RecoveryCodeRedemptionFailed;
            return error;
        }

        public override IdentityError LoginAlreadyAssociated()
        {
            var error = base.LoginAlreadyAssociated();
            error.Description = IdentityResource.LoginAlreadyAssociated;
            return error;
        }

        public override IdentityError InvalidUserName(string userName)
        {
            var error = base.InvalidUserName(userName);
            error.Description = string.Format(IdentityResource.InvalidUserName, userName);
            return error;
        }

        public override IdentityError InvalidEmail(string email)
        {
            var error = base.InvalidEmail(email);
            error.Description = string.Format(IdentityResource.InvalidEmail, email);
            return error;
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            var error = base.DuplicateUserName(userName);
            error.Description = string.Format(IdentityResource.DuplicateUserName, userName);
            return error;
        }

        public override IdentityError DuplicateEmail(string email)
        {
            var error = base.DuplicateEmail(email);
            error.Description = string.Format(IdentityResource.DuplicateEmail, email);
            return error;
        }

        public override IdentityError InvalidRoleName(string role)
        {
            var error = base.InvalidRoleName(role);
            error.Description = string.Format(IdentityResource.InvalidRoleName, role);
            return error;
        }

        public override IdentityError DuplicateRoleName(string role)
        {
            var error = base.DuplicateRoleName(role);
            error.Description = string.Format(IdentityResource.DuplicateRoleName, role);
            return error;
        }

        public override IdentityError UserAlreadyHasPassword()
        {
            var error = base.UserAlreadyHasPassword();
            error.Description = IdentityResource.UserAlreadyHasPassword;
            return error;
        }

        public override IdentityError UserLockoutNotEnabled()
        {
            var error = base.UserLockoutNotEnabled();
            error.Description = IdentityResource.UserLockoutNotEnabled;
            return error;
        }

        public override IdentityError UserAlreadyInRole(string role)
        {
            var error = base.UserAlreadyInRole(role);
            error.Description = string.Format(IdentityResource.UserAlreadyInRole, role);
            return error;
        }

        public override IdentityError UserNotInRole(string role)
        {
            var error = base.UserNotInRole(role);
            error.Description = string.Format(IdentityResource.UserNotInRole, role);
            return error;
        }

        public override IdentityError PasswordTooShort(int length)
        {
            var error = base.PasswordTooShort(length);
            error.Description = string.Format(IdentityResource.PasswordTooShort, length);
            return error;
        }

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        {
            var error = base.PasswordRequiresUniqueChars(uniqueChars);
            error.Description = string.Format(IdentityResource.PasswordRequiresUniqueChars, uniqueChars);
            return error;
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            var error = base.PasswordRequiresNonAlphanumeric();
            error.Description = IdentityResource.PasswordRequiresNonAlphanumeric;
            return error;
        }

        public override IdentityError PasswordRequiresDigit()
        {
            var error = base.PasswordRequiresDigit();
            error.Description = IdentityResource.PasswordRequiresDigit;
            return error;
        }

        public override IdentityError PasswordRequiresLower()
        {
            var error = base.PasswordRequiresLower();
            error.Description = IdentityResource.PasswordRequiresLower;
            return error;
        }

        public override IdentityError PasswordRequiresUpper()
        {
            var error = base.PasswordRequiresUpper();
            error.Description = IdentityResource.PasswordRequiresUpper;
            return error;
        }
    }
}