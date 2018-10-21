using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discussion.Web.Services.Emailconfirmation
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email,string emailTo, string subject, string message);
    }
}
