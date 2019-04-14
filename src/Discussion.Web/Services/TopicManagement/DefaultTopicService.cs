using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Time;
using Discussion.Web.Services.UserManagement.Exceptions;
using Discussion.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Discussion.Web.Services.TopicManagement
{
    public class DefaultTopicService : ITopicService
    {
        private readonly SiteSettings _settings;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<Topic> _topicRepo;
        private readonly IRepository<Reply> _replyRepo;
        private readonly IClock _clock;

        public DefaultTopicService(SiteSettings settings, 
            ICurrentUser currentUser, 
            IRepository<Topic> topicRepo, 
            IRepository<Reply> replyRepo, IClock clock)
        {
            _settings = settings;
            _currentUser = currentUser;
            _topicRepo = topicRepo;
            _replyRepo = replyRepo;
            _clock = clock;
        }

        public TopicViewModel ViewTopic(int topicId)
        {
            var topic = _topicRepo.All()
                .Where(t => t.Id == topicId)
                .Include(t => t.CreatedByUser)
                    .ThenInclude(u => u.AvatarFile)
                .SingleOrDefault();
            
            if (topic == null)
            {
                return null;
            }
            
            var replies = _replyRepo.All()
                .Where(c => c.TopicId == topicId)
                .OrderBy(c => c.CreatedAtUtc)
                .Include(r => r.CreatedByUser)
                    .ThenInclude(u => u.AvatarFile)
                .Include(r => r.CreatedByWeChatAccount)
                    .ThenInclude(wx => wx.User)
                    .ThenInclude(u => u.AvatarFile)
                .ToList();

            // todo:   _eventBus.Publish(new TopicViewedEvent{ TopicId = topicId });
            topic.ViewCount += 1;
            _topicRepo.Update(topic);
            
            return  TopicViewModel.CreateFrom(topic, replies);
        }

        public Topic CreateTopic(TopicCreationModel model)
        {
            if (!_settings.CanCreateNewTopics())
            {
                throw new FeatureDisabledException();
            }
            
            var user = _currentUser.DiscussionUser;
            if (_settings.RequireUserPhoneNumberVerified && !user.PhoneNumberId.HasValue)
            {
                throw new UserVerificationRequiredException();
            }

            // ReSharper disable once PossibleInvalidOperationException
            var topic = new Topic
            {
                Title = model.Title,
                Content = model.Content,
                Type = model.Type.Value,
                CreatedBy = user.Id,
                CreatedAtUtc = _clock.Now.UtcDateTime
            };
            _topicRepo.Save(topic);

            return topic;
        }
    }
}