
export class SiteSettings {
  requireUserPhoneNumberVerified : boolean;
  publicHostName : string;

  enableNewUserRegistration : boolean = true;
  enableNewTopicCreation  : boolean = true;
  enableNewReplyCreation  : boolean = true;
  isReadonly  : boolean;

  footerNoticeLeft : string;
  footerNoticeRight : string;

  headerLink1Text : string;
  headerLink1Url : string;

  headerLink2Text : string;
  headerLink2Url : string;

  headerLink3Text : string;
  headerLink3Url : string;

  headerLink4Text : string;
  headerLink4Url : string;
}
