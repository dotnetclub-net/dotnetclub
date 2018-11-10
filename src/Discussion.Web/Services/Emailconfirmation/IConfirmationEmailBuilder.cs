namespace Discussion.Web.Services.EmailConfirmation
{
    public interface IConfirmationEmailBuilder
    {
        string BuildEmailBody(string callbackUrl);
    }
}