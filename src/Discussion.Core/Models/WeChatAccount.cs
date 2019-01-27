using System.ComponentModel.DataAnnotations.Schema;
using Discussion.Core.Models.UserAvatar;

namespace Discussion.Core.Models
{
    public class WeChatAccount : Entity, IAuthor
    {
        public string WxId { get; set; }
        public string WxAccount { get; set; }
        public string NickName { get; set; }

        [ForeignKey("AvatarFileId")]
        public FileRecord AvatarFile { get; set; }
        public int? AvatarFileId { get; set; }
        [NotMapped]
        public string DisplayName => NickName;
        
        public IUserAvatar GetAvatar()
        {
            throw new System.NotImplementedException();
        }
    }
}