using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Discussion.Web.ViewModels
{
    public class TopicCreationModel
    {
        [Required]
        [MaxLength(255)]
        [DisAllowHtmlTags]
        public string Title { get; set; }

        [Required]
        [MaxLength(200000)]
        [DisAllowHtmlTags]
        public string Content { get; set; }

    }


    class DisAllowHtmlTagsAttribute: ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var invalidPatterns = new []
            {
                new Regex(@"\</", RegexOptions.Compiled),
                new Regex(@"\<[a-z]+", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"&#(x[\da-f]+|\d+);", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

            if(value == null)
            {
                return true;
            }

            var stringValue = (value is string) ? (value as string) : value.ToString();
            return invalidPatterns.AsParallel().All(pattern => !pattern.IsMatch(stringValue));
        }
    }
}
