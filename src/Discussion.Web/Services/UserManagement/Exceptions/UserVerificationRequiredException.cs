using System;

namespace Discussion.Web.Services.UserManagement.Exceptions
{
    public class UserVerificationRequiredException: InvalidOperationException
    {
        public UserVerificationRequiredException() 
            : base("用户账号未通过实名手机号验证，而网站要求必须验证后才能添加内容")
        {

        }
    }
}