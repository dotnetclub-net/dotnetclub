using System.ComponentModel.DataAnnotations;
using Discussion.Core.Mvc;

namespace Discussion.Web.ViewModels
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "Token不能为空")]
        public string Token { get; set; }

        [Required(ErrorMessage = "必须填写新的密码")]
        [MinLength(6, ErrorMessage = "密码最少包含 6 个字符")]
        [MaxLength(20, ErrorMessage = "密码最多包含 20 个字符")]
        [PasswordRules]
        public string Password { get; set; }

        public int UserId { get; set; }

        public bool Succeeded { get; set; }
    }
}
