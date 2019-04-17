using System.ComponentModel.DataAnnotations.Schema;
using Discussion.Core.Models.UserAvatar;

namespace Discussion.Core.Models
{
    public class WeChatAccount : Entity, IAuthor
    {
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User User { get; set; }
        
        public string WxId { get; set; }
        public string WxAccount { get; set; }

        [NotMapped]
        public string DisplayName => NickName;
        public string NickName { get; set; }

        public IUserAvatar GetAvatar()
        {
            if (UserId > 0 && User != null)
            {
                return User.GetAvatar();
            }
            
            return new GravatarAvatar { EmailAddress = $"{WxId}@wechat-user.com"};
        }
    }
}