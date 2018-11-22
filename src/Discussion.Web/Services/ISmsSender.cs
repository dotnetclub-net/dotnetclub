using System.Threading.Tasks;

namespace Discussion.Web.Services
{
    public interface ISmsSender
    {
        Task SendVerificationCodeAsync(string phoneNumber, string code);
    }
}