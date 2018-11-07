using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discussion.Web.ViewModels
{
    public class EmailSendTokenViewModel
    {
        public string UserId { get; set; }
        public DateTime SendTime => DateTime.Now;
        public string Tag => "dotnetclub";
    }
}
