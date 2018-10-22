using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Discussion.Web.ViewModels
{
    public class EmailSettingViewModel
    {
        [Required(ErrorMessage = "请输入电子邮件")]
        [EmailAddress(ErrorMessage = "电子邮件地址格式不正确！")]
        public string EmailAddress { get; set; }
    }
}
