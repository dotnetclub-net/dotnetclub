using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
using Discussion.Tests.Common;

namespace Discussion.Web.Tests.Fixtures
{
    public class TopicBuilder
    {
        private readonly TestDiscussionWebApp _app;
        private readonly Topic _topic = new Topic();
        private readonly List<Reply> _replies = new List<Reply>();

        public TopicBuilder(TestDiscussionWebApp app)
        {
            _app = app;
            
            _topic.Title = "dummy topic title " + StringUtility.Random(6);  
            _topic.Content = "**dummy topic**\n\n content " + StringUtility.Random(12);
            _topic.Type = TopicType.Discussion;
        }

        public TopicBuilder WithTitle(string title)
        {
            _topic.Title = title;
            return this;
        }
        
        public TopicBuilder WithAuthor(User user)
        {
            _topic.Author = user;
            return this;
        }
        
        public TopicBuilder WithContent(string content)
        {
            _topic.Content = content;
            return this;
        }
        
        public TopicBuilder WithType(TopicType topicType)
        {
            _topic.Type = topicType;
            return this;
        }
        
        public TopicBuilder WithReply(string content = null, User user = null)
        {
            _topic.LastRepliedAt = DateTime.UtcNow.AddMinutes(-3);
            _replies.Add(new Reply
            {
                CreatedByUser = user,
                Content = content ?? "dummy reply " + StringUtility.Random(20)
            });
            return this;
        }
        
        public TopicBuilder With(Action<Topic> topicCreator)
        {
            topicCreator(_topic);
            return this;
        }
        
        public Topic Create()
        {
            var someUser = _app.CreateUser(StringUtility.Random());
            
            var topicRepo = _app.GetService<IRepository<Topic>>();
            if (_topic.Author == null)
            {
                _topic.Author = someUser;
            }
            
            if (_replies.Count > 0)
            {
                _topic.LastRepliedUser = _replies.LastOrDefault(r => r.CreatedByUser != null)?.CreatedByUser ?? someUser;
            }
            
            
            topicRepo.Save(_topic);

            var replyRepo = _app.GetService<IRepository<Reply>>();
            _replies.ForEach(reply =>
            {
                reply.TopicId = _topic.Id;
                reply.CreatedByUser = _topic.Author ?? someUser;
                replyRepo.Save(reply);
            });
            return _topic;
        }
    }

    public static class TestApplicationExtensions
    {
        public static TopicBuilder NewTopic(this TestDiscussionWebApp app, 
            string title = null,
            string content = null,
            TopicType type = TopicType.Discussion)
        {
            var tb = new TopicBuilder(app).WithType(type);
            
            if (title != null)
                tb.WithTitle(title);
            if (content != null)
                tb.WithContent(content);
            
            return tb;
        }
    }
}