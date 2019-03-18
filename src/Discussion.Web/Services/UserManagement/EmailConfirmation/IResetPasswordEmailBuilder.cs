using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discussion.Web.Services.UserManagement.EmailConfirmation
{
    public interface IResetPasswordEmailBuilder
    {
        string BuildEmailBody(string displayName, string resetUrl);
    }
}
