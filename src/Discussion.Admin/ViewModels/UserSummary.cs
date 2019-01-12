using System;

namespace Discussion.Admin.ViewModels
{
    public class UserSummary
    {
        public int Id { get; set; }
        public string LoginName { get; set; }
        public string DisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? BlockToDate { get; set; }
        public string AvatarUrl { get; set; }
    }
}