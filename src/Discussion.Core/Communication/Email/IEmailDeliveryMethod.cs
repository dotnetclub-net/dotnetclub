using System.Threading.Tasks;

namespace Discussion.Core.Communication.Email
{
    public interface IEmailDeliveryMethod
    {
        Task SendEmailAsync(string emailTo, string subject, string message);
    }
}
