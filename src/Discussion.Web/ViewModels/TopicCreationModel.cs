using System.ComponentModel.DataAnnotations;

namespace Discussion.Web.ViewModels
{
    public class TopicCreationModel
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        [MaxLength(200000)]
        public string Content { get; set; }

    }
}
