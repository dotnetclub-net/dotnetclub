using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Discussion.Web.ViewModels
{
    public class SigninUserViewModel
    {
        [DisplayName("用户名")]
        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        [RegularExpression(@"[a-zA-Z0-9\-_]+")]   // 仅能包含大小写字母、数字，以及 -_
        public string UserName { get; set; }

        [DisplayName("密码")]
        [Required]
        [PasswordRules]
        [MinLength(6)]
        [MaxLength(20)]
        public string Password { get; set; }
    }
    
    
    
    /// <summary>
    /// 密码规则：必须包含以下各类中的两种：大写字母、小写字母，数字，键盘特殊字符
    /// </summary>
    class PasswordRulesAttribute : ValidationAttribute
    {
       
        static readonly Regex _symboles = new Regex(@"[\x20,<\.>/\?;:'""\[\{\]\}\\\|`~\!@#\$%\^&\*\(\)\-_\=\+]+", RegexOptions.Compiled);
        static readonly Regex _numbers = new Regex("[0-9]+", RegexOptions.Compiled);
        static readonly Regex _lowerCase = new Regex("[a-z]+", RegexOptions.Compiled);
        static readonly Regex _upperCase = new Regex("[A-Z]+", RegexOptions.Compiled);

        public PasswordRulesAttribute()
        {
            ErrorMessage = "必须包含以下各类中的两种：大写字母、小写字母，数字，键盘特殊字符";
        }
        
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                // 此处不检查空值，空值留给 Required 检查
                return true;
            }

            var password = Convert.ToString(value);
            var contains = 0;
            if (_symboles.IsMatch(password))
            {
                contains++;
            }
            
            if (_numbers.IsMatch(password))
            {
                contains++;
            }           
            
            if (_lowerCase.IsMatch(password))
            {
                contains++;
            }
            
            if (_upperCase.IsMatch(password))
            {
                contains++;
            }

            return contains >= 2;
        }
    }

}