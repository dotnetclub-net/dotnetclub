namespace Discussion.Web.Services
{
    public class IdentityServerOptions
    {
        
        public bool IsEnable { get; set; }

        public string Authority { get; set; }

        public string RequireHttpsMetadata { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
        
        public string RegisterUri { get; set; }

        public string SettingsUri { get; set; }
    }

    public enum IdentityAction
    {
        Signin,
        SignOut,
        Register,
        Settings
    }
}