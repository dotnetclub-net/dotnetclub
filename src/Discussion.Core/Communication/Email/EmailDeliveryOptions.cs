namespace Discussion.Core.Communication.Email
{
    public class EmailDeliveryOptions
    {
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string MailFrom { get; set; }
        public string ServerHost { get; set; }
        public int ServerSslPort { get; set; }
    }
}
