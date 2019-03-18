using System;

namespace Discussion.Web.Services.UserManagement.Exceptions
{
    public class RetrievePasswordVerificationException : InvalidOperationException
    {
        public RetrievePasswordVerificationException(string message) : base(message)
        {
        }
    }
}
