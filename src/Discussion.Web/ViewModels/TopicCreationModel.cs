using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Discussion.Core.Models;

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
        public string Content { get; set; }

        [Required]
        [MustBeDiscribed]
        public TopicType? Type { get; set; }
    }

    internal class DisAllowHtmlTagsAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var invalidPatterns = new[]
            {
                new Regex(@"\</", RegexOptions.Compiled),
                new Regex(@"\<[a-z]+", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"&#(x[\da-f]+|\d+);", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

            if (value == null)
            {
                return true;
            }

            var stringValue = (value is string) ? (value as string) : value.ToString();
            return invalidPatterns.AsParallel().All(pattern => !pattern.IsMatch(stringValue));
        }
    }

    internal class MustBeDiscribedAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                // 此处不检查空值，空值留给 Required 检查
                return true;
            }

            TopicType topicType = 0;
            var isInt = (value is int);
            if (isInt)
            {
                var intVal = (int)value;
                topicType = (TopicType)intVal;
            }

            var isString = (value is string);
            if (isString)
            {
                var strValue = (string)value;
                if (!Enum.TryParse(strValue, out topicType))
                {
                    return false;
                }
            }

            var isTopicType = (value is TopicType);
            if (isTopicType)
            {
                topicType = (TopicType)value;
            }

            return IsDescribed(topicType);
        }

        private static bool IsDescribed(TopicType topicType)
        {
            var memberInfo = typeof(TopicType).GetMember(topicType.ToString()).FirstOrDefault();
            return memberInfo != null;
        }
    }
}