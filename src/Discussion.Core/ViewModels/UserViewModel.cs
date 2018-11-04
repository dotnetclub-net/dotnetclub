using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Discussion.Core.Mvc;

namespace Discussion.Core.ViewModels
{
    public class UserViewModel
    {
        [DisplayName("用户名")]
        [Required(ErrorMessage = "必须填写用户名")]
        [MinLength(3, ErrorMessage = "用户名最少包含 3 个字符")]
        [MaxLength(20, ErrorMessage = "用户名最多包含 20 个字符")]
        [RegularExpression(@"^[a-zA-Z0-9\-_]+$", ErrorMessage = "用户名只能包含大小写字母、数字，以及短横线（ -_）")]
        public string UserName { get; set; }

        [DisplayName("密码")]
        [Required(ErrorMessage = "必须填写密码")]
        [MinLength(6, ErrorMessage = "密码最少包含 6 个字符")]
        [MaxLength(20, ErrorMessage = "密码最多包含 20 个字符")]
        [PasswordRules]
        public string Password { get; set; }
    }
}