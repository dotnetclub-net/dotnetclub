using System.ComponentModel.DataAnnotations;

namespace Discussion.Web.ViewModels
{
    public class CommentCreationModel
    {
        [Required(ErrorMessage = "必须填写评论内容")]
        [MaxLength(3000, ErrorMessage = "评论不能超过 3000 个字符")]
        public string Content { get; set; }
    }
}