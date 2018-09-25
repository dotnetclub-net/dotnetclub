using System.ComponentModel.DataAnnotations;

namespace Discussion.Web.ViewModels
{
    public class CommentCreationModel
    {
        [Required(ErrorMessage = "必须填写评论内容")]
        public string Content { get; set; }
    }
}