using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Discussion.Web.Services.EmailConfirmation
{
    public class EmailSender : IEmailSender
    {
        private readonly string smtpServer;
        private readonly string mailFrom;
        private readonly string mailPwd;
        private AuthMessageSenderOptions _authMessageSenderOptions;
        public EmailSender(IOptions<AuthMessageSenderOptions> authMessageSenderOptions)
        {
            smtpServer = authMessageSenderOptions.Value.SendServer;
            mailFrom = authMessageSenderOptions.Value.SendGridUser;
            mailPwd = authMessageSenderOptions.Value.SendGridKey;
        }
        
        public Task SendEmailAsync(string emailTo, string subject, string message)
        {
            return Execute(emailTo,subject, message);
        }
        
        public Task Execute(string emailTo, string subject,string message)
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
            return Task.Run(() => {
                smtpClient.Send(mailMessage);
            });
        }
    }
}
