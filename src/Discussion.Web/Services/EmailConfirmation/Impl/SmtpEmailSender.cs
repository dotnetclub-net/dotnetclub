using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Discussion.Web.Services.EmailConfirmation.Impl
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSendingOptions _emailSendingOptions;
        public SmtpEmailSender(IOptions<EmailSendingOptions> emailSendingOptions)
        {
            _emailSendingOptions = emailSendingOptions.Value;
        }
        
        public async Task SendEmailAsync(string emailTo, string subject, string message)
        {
            var smtpClient = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Host = _emailSendingOptions.ServerHost,
                Port = _emailSendingOptions.ServerSslPort,
                EnableSsl = true
            };

            if (!string.IsNullOrEmpty(_emailSendingOptions.LoginName)
                && !string.IsNullOrEmpty(_emailSendingOptions.Password))
            {
                smtpClient.Credentials = new System.Net.NetworkCredential(_emailSendingOptions.LoginName, _emailSendingOptions.Password);
            }

            var fromAddress = string.IsNullOrEmpty(_emailSendingOptions.MailFrom)
                ? _emailSendingOptions.LoginName
                : _emailSendingOptions.MailFrom;
            var mailMessage = new MailMessage(fromAddress, emailTo)
            {
                Subject = subject,
                Body = message,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };
            await Task.Run(() => {
                smtpClient.Send(mailMessage);
            });
        }
    }
}
