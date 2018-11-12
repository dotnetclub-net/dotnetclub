using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Discussion.Web.Services.EmailConfirmation.Impl
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly string smtpServer;
        private readonly string mailFrom;
        private readonly string mailPwd;
        private AuthMessageSenderOptions _authMessageSenderOptions;
        public SendGridEmailSender(IOptions<AuthMessageSenderOptions> authMessageSenderOptions)
        {
            smtpServer = authMessageSenderOptions.Value.SendServer;
            mailFrom = authMessageSenderOptions.Value.SendGridUser;
            mailPwd = authMessageSenderOptions.Value.SendGridKey;
        }
        
        public async Task SendEmailAsync(string emailTo, string subject, string message)
        {
            // 邮件服务设置
            SmtpClient smtpClient = new SmtpClient(smtpServer, 25);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;                      
            smtpClient.Host = smtpServer;                                                
            smtpClient.Credentials = new System.Net.NetworkCredential(mailFrom, mailPwd);
            smtpClient.EnableSsl = true;                                                         


            // 发送邮件设置        
            MailMessage mailMessage = new MailMessage(mailFrom, emailTo);               
            mailMessage.Subject = subject;
            mailMessage.Body = message;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.IsBodyHtml = true;
            mailMessage.Priority = MailPriority.Low;
            await Task.Run(() => {
                smtpClient.Send(mailMessage);
            });
        }
    }
}
