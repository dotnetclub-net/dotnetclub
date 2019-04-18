using System.ComponentModel.DataAnnotations.Schema;
using Discussion.Core.Models.UserAvatar;
using Discussion.Core.Utilities;

namespace Discussion.Core.Models
{
    public class WeChatAccount : Entity, IAuthor
    {
        public int UserId { get; set; }
        
        public string WxId { get; set; }
        public string WxAccount { get; set; }

        [NotMapped]
        public string DisplayName => RandomDisplayNameGenerator.Generate();

        public IUserAvatar GetAvatar()
        {
            return new GravatarAvatar{ EmailAddress = $"{WxId}@wechat-user.com"};
        }
    }
}