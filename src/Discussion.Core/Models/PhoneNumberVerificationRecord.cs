using System;

namespace Discussion.Core.Models
{
    public class PhoneNumberVerificationRecord: Entity
    {
        public int UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public DateTime Expires { get; set; }
    }
}