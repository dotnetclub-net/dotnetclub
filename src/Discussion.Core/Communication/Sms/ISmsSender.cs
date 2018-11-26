using System.Threading.Tasks;

namespace Discussion.Core.Communication.Sms
{
    public interface ISmsSender
    {
        Task SendVerificationCodeAsync(string phoneNumber, string code);
    }
}