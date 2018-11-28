using System;

namespace Discussion.Web.Services.UserManagement.Exceptions
{
    public class UserEmailAlreadyConfirmedException : InvalidOperationException
    {
        public UserEmailAlreadyConfirmedException(string userName) : base("用户的邮件已经确认，不需要再次确认")
        {
            this.UserName = userName;
        }
        
        public string UserName { get; set; }
    }
}