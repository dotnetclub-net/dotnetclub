using System.ComponentModel.DataAnnotations;

namespace Discussion.Web.ViewModels
{
    public class UserSettingsViewModel
    {
        [EmailAddress(ErrorMessage = "邮件地址格式不正确")]
        public string EmailAddress { get; set; }

        public string DisplayName { get; set; }
        public int AvatarFileId { get; set; }
    }
}
