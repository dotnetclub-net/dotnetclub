using System;

namespace Discussion.Core.Models
{
    public class User : Entity, IUser
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }

        public string HashedPassword { get; set; }
        public DateTime? LastSeenAt { get; set; }
    }


    public interface IUser
    {
        int Id { get; }
        string UserName { get; set; }
        string DisplayName { get; }
    }

    public class Role
    {
        public string Name { get; }
    }
}
