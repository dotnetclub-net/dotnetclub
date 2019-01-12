namespace Discussion.Core.Models
{
    public class SiteSettings : Entity
    {
        public bool RequireUserPhoneNumberVerified { get; set; }
        public string PublicHostName { get; set; }
        
        public bool EnableNewUserRegistration {get;set;}
        public bool EnableNewTopicCreation  {get;set;}
        public bool EnableNewReplyCreation  {get;set;}
        public bool IsReadonly  {get;set;}
        
        public string FooterNoticeLeft {get;set;}
        public string FooterNoticeRight {get;set;}
        
        public string HeaderLink1Text {get;set;}
        public string HeaderLink1Url {get;set;}
        
        public string HeaderLink2Text {get;set;}
        public string HeaderLink2Url {get;set;}
        
        public string HeaderLink3Text {get;set;}
        public string HeaderLink3Url {get;set;}
        
        public string HeaderLink4Text {get;set;}
        public string HeaderLink4Url {get;set;}
    }
}