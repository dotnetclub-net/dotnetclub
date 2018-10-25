using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Discussion.Web.Services.Emailconfirmation
{
    public class EmailSender : IEmailSender
    {
        private const string smtpServer = "smtp.163.com";
        private const string mailFrom = "13671241939@163.com";
        private const string mailPwd = "lmc1995.!";
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
