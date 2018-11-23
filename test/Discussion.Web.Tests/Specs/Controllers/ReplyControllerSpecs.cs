using System;
using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Controllers;
using Discussion.Web.Models;
using Discussion.Web.Services.Identity;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class ReplyControllerSpecs
    {
        private readonly TestDiscussionWebApp _app;

        public ReplyControllerSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
        }

        [Fact]
        public void should_add_reply()
        {
            // Arrange
            _app.MockUser();
            var (topic, userId) = CreateTopic(_app);

            // Act
            var replyController = _app.CreateController<ReplyController>();
            replyController.Reply(topic.Id, new ReplyCreationModel
            {
                Content = "my reply"
            });

            // Assert
            var replies = _app.GetService<IRepository<Reply>>()
                        .All()
                        .Where(c => c.TopicId == topic.Id)
                        .ToList();
            replies.Count.ShouldEqual(1);
            replies[0].TopicId.ShouldEqual(topic.Id);
            replies[0].CreatedBy.ShouldEqual(userId);
            replies[0].Content.ShouldEqual("my reply");

            var dbContext = _app.GetService<ApplicationDbContext>();
            dbContext.Entry(topic).Reload();
            topic.ReplyCount.ShouldEqual(1);
            topic.LastRepliedUser.ShouldNotBeNull();
            topic.LastRepliedAt.ShouldNotBeNull();
            var span = DateTime.UtcNow - topic.LastRepliedAt.Value;
            Assert.True(span.TotalSeconds > 0);
            Assert.True(span.TotalSeconds < 10);
        }

        [Fact]
        public void should_not_add_reply_when_model_state_invalid()
        {
            // Arrange
            _app.MockUser();
            var (topic, userId) = CreateTopic(_app);

            var replyController = _app.CreateController<ReplyController>();
            replyController.ModelState.AddModelError("Content", "必须填写回复内容");

            // Act
            var replyResult = replyController.Reply(topic.Id, new ReplyCreationModel
            {
                Content = "my reply"
            });

            // Assert
            var statusCodeResult = replyResult as BadRequestObjectResult;
            Assert.NotNull(statusCodeResult);
            Assert.Equal(400, statusCodeResult.StatusCode);

            var errors = statusCodeResult.Value as SerializableError;
            Assert.NotNull(errors);
            Assert.Contains("必须填写回复内容", errors.Values.Cast<string[]>().SelectMany(err => err).ToList());
        }

        [Fact]
        public void should_not_add_reply_if_require_verified_phone_number_but_user_has_no()
        {
            _app.MockUser();
            var (topic, _) = CreateTopic(_app);
            var replyRepo = new Mock<IRepository<Reply>>();
            var siteSettings = new SiteSettings {RequireUserPhoneNumberVerified = true};

            var replyController = new ReplyController(replyRepo.Object, _app.GetService<IRepository<Topic>>(), siteSettings);
            replyController.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = _app.User,
                RequestServices = _app.ApplicationServices
            };

            var actionResult = replyController.Reply(topic.Id, new ReplyCreationModel { Content = "some content"});
            
            actionResult.IsType<BadRequestResult>();
            replyRepo.VerifyNoOtherCalls();
        }
        
        [Fact]
        public void should_not_add_reply_when_topic_id_does_not_exist()
        {
            _app.MockUser();

            var replyController = _app.CreateController<ReplyController>();

            var replyResult = replyController.Reply(99999, new ReplyCreationModel
            {
                Content = "my reply"
            });

            var statusCodeResult = replyResult as BadRequestObjectResult;
            Assert.NotNull(statusCodeResult);
            Assert.Equal(400, statusCodeResult.StatusCode);
        }

        internal static (Topic, int) CreateTopic(TestDiscussionWebApp testDiscussionWeb)
        {
            var userId = testDiscussionWeb.User.ExtractUserId().Value;
            var topic = new Topic
            {
                Title = "test topic",
                Content = "topic content",
                CreatedBy = userId,
                Type = TopicType.Discussion
            };
            testDiscussionWeb.GetService<IRepository<Topic>>().Save(topic);
            return (topic, userId);
        }
    }
}