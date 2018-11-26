using System.Threading.Tasks;

namespace Discussion.Web.Services.UserManagement.PhoneNumberVerification
{
    public interface IPhoneNumberVerificationService
    {
        bool IsFrequencyExceededForUser(int userId);
        Task SendVerificationCodeAsync(int userId, string phoneNumber);
        string GetVerifiedPhoneNumberByCode(int userId, string providedVerificationCode);
    }
}