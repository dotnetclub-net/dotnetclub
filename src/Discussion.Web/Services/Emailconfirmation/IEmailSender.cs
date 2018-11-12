using System.Threading.Tasks;

namespace Discussion.Web.Services.EmailConfirmation
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string emailTo, string subject, string message);
    }
}
