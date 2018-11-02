using Discussion.Admin.Models;
using Discussion.Core.Models;

namespace Discussion.Admin.Services
{
    public interface IAdminUserService
    {
        IssuedToken IssueJwtToken(AdminUser adminUser);
        bool VerifyPassword(AdminUser adminUser, string providedPassword);
        string HashPassword(string password);
    }
}