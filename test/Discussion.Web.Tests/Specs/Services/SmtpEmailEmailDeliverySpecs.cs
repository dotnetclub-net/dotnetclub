using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discussion.Core.Communication.Email;
using Discussion.Core.Communication.Email.DeliveryMethods;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    public class SmtpEmailEmailDeliverySpecs
    {
        // ReSharper disable PossibleNullReferenceException
        
        [Fact]
        public async Task should_deliver_emails()
        {
            MimeMessage sentMessage = null;
            var emailDelivery = new SmtpEmailEmailDelivery(MockOptions().Object, new SpySmtpClient(msg => sentMessage = msg));

            await emailDelivery.SendEmailAsync("user@dotnetclub.net", "激活邮件", "<html>content</html>");

            Assert.NotNull(sentMessage);
            Assert.Equal("user@dotnetclub.net", (sentMessage.To.First() as MailboxAddress).Address);
            Assert.Equal("system@from.com", (sentMessage.From.First() as MailboxAddress).Address);
            Assert.Equal("激活邮件", sentMessage.Subject);
            Assert.Equal("<html>content</html>", sentMessage.HtmlBody);
        }

        private static Mock<IOptions<EmailDeliveryOptions>> MockOptions()
        {
            var options = new Mock<IOptions<EmailDeliveryOptions>>();
            options.SetupGet(op => op.Value).Returns(new EmailDeliveryOptions()
            {
                ServerHost = "smtp.dotnetclub.net",
                ServerSslPort = 465,
                LoginName = "test",
                Password = "pwd",
                MailFrom = "system@from.com"
            });
            return options;
        }

        class SpySmtpClient : SmtpClient
        {
            private readonly Action<MimeMessage> _onMessage;

            public SpySmtpClient(Action<MimeMessage> onMessage)
            {
                _onMessage = onMessage;
            }
            
            
            public override Task ConnectAsync(string host, int port = 0,
                SecureSocketOptions options = SecureSocketOptions.Auto,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }


            public override Task AuthenticateAsync(Encoding encoding, ICredentials credentials,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }


            public override Task DisconnectAsync(bool quit,
                CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }

            public override Task SendAsync(MimeMessage message,
                CancellationToken cancellationToken = default(CancellationToken), ITransferProgress progress = null)
            {
                this._onMessage?.Invoke(message);
                return Task.CompletedTask;
            }
        }
        
    }
}