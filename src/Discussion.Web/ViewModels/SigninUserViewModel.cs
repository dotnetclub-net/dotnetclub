using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Discussion.Web.ViewModels
{
    public class SigninUserViewModel
    {
        [DisplayName("用户名")]
        [Required]
        public string UserName { get; set; }

        [DisplayName("密码")]
        [Required]
        public string Password { get; set; }
    }
}