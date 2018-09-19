using Discussion.Web.Data;
using System;
using System.Security.Claims;

namespace Discussion.Web.Models
{
    public class User : Entity, IUser
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }

        public string HashedPassword { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastSeendAt { get; set; }
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

    public class DiscussionPrincipal : ClaimsPrincipal
    {
        public DiscussionPrincipal(ClaimsIdentity identity) :base(identity)
        {

        }

        public IUser User { get; set; }
    }
}
