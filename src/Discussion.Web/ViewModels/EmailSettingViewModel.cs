using System.ComponentModel.DataAnnotations;

namespace Discussion.Web.ViewModels
{
    public class EmailSettingViewModel
    {
        [EmailAddress(ErrorMessage = "电子邮件地址格式不正确！")]
        public string EmailAddress { get; set; }
    }
}
