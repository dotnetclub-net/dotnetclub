namespace Discussion.Web.Services.UserManagement.EmailConfirmation
{
    public interface IConfirmationEmailBuilder
    {
        string BuildEmailBody(string displayName, string callbackUrl);
    }
}