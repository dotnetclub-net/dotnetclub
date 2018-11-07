using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discussion.Web.Services.Emailconfirmation
{
    public class AuthMessageSenderOptions
    {
        public string SendGridUser { get; set; }
        public string SendGridKey { get; set; }
        public string SendServer { get; set; }
        public string EncryptionKey { get; set; }
    }
}
