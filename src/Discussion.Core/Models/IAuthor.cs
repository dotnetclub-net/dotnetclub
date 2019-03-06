using Discussion.Core.Models.UserAvatar;

namespace Discussion.Core.Models
{
    public interface IAuthor
    {
        string DisplayName { get; }
        IUserAvatar GetAvatar();
    }
}