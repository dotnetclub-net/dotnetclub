using System;

namespace Discussion.Core.Models
{
    public class User : Entity, IUser
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }

        public string HashedPassword { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public bool EmailAddressConfirmed { get; set; }
        public int AvatarFileId { get; set; }

        public string ConfirmedEmail => EmailAddressConfirmed ? EmailAddress : null;
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
