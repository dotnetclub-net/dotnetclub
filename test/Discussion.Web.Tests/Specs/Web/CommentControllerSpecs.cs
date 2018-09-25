using System.Linq;
using Discussion.Web.Controllers;
using Discussion.Web.Data;
using Discussion.Web.Models;
using Discussion.Web.Services.Identity;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Discussion.Web.Tests.Specs.Web
{
    [Collection("AppSpecs")]
    public class CommentControllerSpecs
    {
        private TestApplication _app;
        public CommentControllerSpecs(TestApplication app)
        {
            _app = app.Reset();
        }


        [Fact]
        public void should_add_comment()
        {
            _app.MockUser();
            var (topic, userId) = CreateTopic(_app);

            var commentController = _app.CreateController<CommentController>();
            commentController.Comment(topic.Id, new CommentCreationModel
            {
                Content = "my comment"
            });
            
            
            var allComments = _app.GetService<IRepository<Comment>>()
                        .All()
                        .Where(c => c.TopicId == topic.Id)
                        .ToList();
            allComments.Count.ShouldEqual(1);
            allComments[0].TopicId.ShouldEqual(topic.Id);
            allComments[0].CreatedBy.ShouldEqual(userId);
            allComments[0].Content.ShouldEqual("my comment");
        }
        
        [Fact]
        public void should_not_add_comment_when_model_state_invalid()
        {
            _app.MockUser();
            var (topic, userId) = CreateTopic(_app);

            var commentController = _app.CreateController<CommentController>();
            commentController.ModelState.AddModelError("Content", "必须填写评论内容");
            
            var commentResult = commentController.Comment(topic.Id, new CommentCreationModel
            {
                Content = "my comment"
            });

            var statusCodeResult = commentResult as StatusCodeResult;
            Assert.NotNull(statusCodeResult);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }
        
        [Fact]
        public void should_not_add_comment_when_topic_id_does_not_exist()
        {
            _app.MockUser();

            var commentController = _app.CreateController<CommentController>();
            
            var commentResult = commentController.Comment(99999, new CommentCreationModel
            {
                Content = "my comment"
            });

            var statusCodeResult = commentResult as StatusCodeResult;
            Assert.NotNull(statusCodeResult);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

       
        
        internal static (Topic, int) CreateTopic(TestApplication testApplication)
        {
            var userId = testApplication.User.ExtractUserId().Value;
            var topic = new Topic
            {
                Title = "test topic",
                Content = "topic content",
                CreatedBy = userId,
                Type = TopicType.Discussion
            };
            testApplication.GetService<IRepository<Topic>>().Save(topic);
            return (topic, userId);
        }
    }


}
