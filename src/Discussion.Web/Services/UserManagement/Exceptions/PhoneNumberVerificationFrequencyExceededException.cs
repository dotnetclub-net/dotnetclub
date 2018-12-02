using System;

namespace Discussion.Web.Services.UserManagement.Exceptions
{
    public class PhoneNumberVerificationFrequencyExceededException : InvalidOperationException
    {
        public PhoneNumberVerificationFrequencyExceededException() 
            : base("验证手机号码操作频次超过了系统限制")
        {

        }
    }
}