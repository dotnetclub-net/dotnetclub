using System.ComponentModel.DataAnnotations;

namespace Discussion.Web.ViewModels
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "必须填写用户名或邮箱")]
        public string UsernameOrEmail { get; set; }
    }
}
