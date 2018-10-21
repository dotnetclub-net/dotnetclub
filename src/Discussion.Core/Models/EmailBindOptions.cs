using System;
using System.Collections.Generic;
using System.Text;

namespace Discussion.Core.Models
{
    public class EmailBindOptions: Entity
    {
        public int UserId { get; set; }
        public string EmailAddress { get; set; }
        public string OldEmailAddress { get; set; }
        public string CallbackToken { get; set; }
        public bool IsActivation { get; set; }
    }
}
