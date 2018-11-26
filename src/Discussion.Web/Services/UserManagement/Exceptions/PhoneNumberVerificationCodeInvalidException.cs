using System;

namespace Discussion.Web.Services.UserManagement.Exceptions
{
    public class PhoneNumberVerificationCodeInvalidException : InvalidOperationException
    {
        public PhoneNumberVerificationCodeInvalidException() 
            : base("手机验证码不正确，或已过期")
        {

        }
    }
}