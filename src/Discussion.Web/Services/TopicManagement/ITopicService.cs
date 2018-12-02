using Discussion.Core.Models;
using Discussion.Web.ViewModels;

namespace Discussion.Web.Services.TopicManagement
{
    public interface ITopicService
    {
        TopicViewModel ViewTopic(int topicId);
        Topic CreateTopic(TopicCreationModel model);
    }
}