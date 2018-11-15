using System.Linq;
using System.Text.RegularExpressions;
using Discussion.Web.Resources;
using Xunit;

namespace Discussion.Web.Tests.Specs.Resources
{
    public class IdentityErrorDescriberSpecs
    {
        [Fact]
        public void should_use_zh_cn_messages()
        {
            var describer = new ResourceBasedIdentityErrorDescriber();
            var messages = new[]
            {
                describer.ConcurrencyFailure().Description,
                describer.DefaultError().Description,
                describer.DuplicateEmail("email@here.com").Description,
                describer.InvalidEmail("2#invalid.com").Description,
                describer.InvalidToken().Description,
                describer.PasswordMismatch().Description,
                describer.DuplicateRoleName("role").Description,
                describer.InvalidUserName("name").Description,
                describer.LoginAlreadyAssociated().Description,
                describer.PasswordRequiresDigit().Description,
                describer.PasswordRequiresLower().Description,
                describer.PasswordRequiresUpper().Description,
                describer.PasswordTooShort(10).Description,
                describer.PasswordRequiresNonAlphanumeric().Description,
                describer.PasswordRequiresUniqueChars(5).Description,
                describer.RecoveryCodeRedemptionFailed().Description,
                describer.UserAlreadyHasPassword().Description,
                describer.UserAlreadyInRole("role").Description,
                describer.UserLockoutNotEnabled().Description,
                describer.UserNotInRole("role").Description
            };

            messages.ToList().ForEach(msg => Assert.True(HasChinese(msg)));
        }

        internal static bool HasChinese(string text)
        {
            if (text.Contains("，") || text.Contains("。"))
            {
                return true;
            }

            var chineseRange = new Regex(@"[\u2E80-\u2FD5\u3190-\u319f\u3400-\u4DBF\u4E00-\u9FCC]");
            return chineseRange.IsMatch(text);
        }
    }
}