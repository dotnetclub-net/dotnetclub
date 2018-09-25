using System.ComponentModel.DataAnnotations;

namespace Discussion.Web.ViewModels
{
    public class ReplyCreationModel
    {
        [Required(ErrorMessage = "必须填写回复内容")]
        [MaxLength(3000, ErrorMessage = "回复不能超过 3000 个字符")]
        public string Content { get; set; }
    }
}