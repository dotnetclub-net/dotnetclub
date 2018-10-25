using System;
using System.Collections.Generic;
using System.Text;

namespace Discussion.Core.Models
{
    public class Eamil : Entity
    {
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string EncryptionToken { get; set; }
        public string ExpireTime { get; set; }
        public bool IsActicated { get; set; }
        public bool IsSubsribe { get; set; }
    }
}
