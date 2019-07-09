using System;
using System.ComponentModel.DataAnnotations.Schema;
using Discussion.Core.Models.UserAvatar;
using Discussion.Core.Time;
using IUserAvatar = Discussion.Core.Models.UserAvatar.IUserAvatar;

namespace Discussion.Core.Models
{
    public class User : Entity, IUser, IAuthor
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }

        [ForeignKey("AvatarFileId")]
        public FileRecord AvatarFile { get; set; }
        public int? AvatarFileId { get; set; }
        
        public string HashedPassword { get; set; }
        public DateTime? LastSeenAt { get; set; }
        
        public bool EmailAddressConfirmed { get; set; }
        public string ConfirmedEmail => EmailAddressConfirmed ? EmailAddress : null;
        
        [ForeignKey("PhoneNumberId")]
        public VerifiedPhoneNumber VerifiedPhoneNumber { get; set; }
        public int? PhoneNumberId { get; set; }
        
        public string OpenId { get; set; }
        
        public string OpenIdProvider { get; set; }
        
        public IUserAvatar GetAvatar()
        {
            if (AvatarFileId > 0)
            {
                return new StorageFileAvatar {  StorageFileSlug = AvatarFile.Slug };
            }

            if (EmailAddressConfirmed)
            {
                return new GravatarAvatar { EmailAddress = EmailAddress };
            }

            return new DefaultAvatar();
        }

        public bool CanModifyPhoneNumberNow(IClock clock)
        {
            if (PhoneNumberId == null)
            {
                return true;
            }
            
            var sevenDaysAgo = clock.Now.UtcDateTime.AddDays(-7);
            return VerifiedPhoneNumber.CreatedAtUtc < sevenDaysAgo
                   && VerifiedPhoneNumber.ModifiedAtUtc < sevenDaysAgo;
        }
    }

    public interface IUser
    {
        int Id { get; }
        string UserName { get; set; }
        string DisplayName { get; }
        string EmailAddress { get; set; }
        bool EmailAddressConfirmed { get; set; }
    }

    public class Role
    {
        public string Name { get; }
    }
}
