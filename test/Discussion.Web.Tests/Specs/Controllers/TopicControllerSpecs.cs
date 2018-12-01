using System;
using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Pagination;
using Discussion.Core.Time;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Controllers;
using Discussion.Web.Models;
using Discussion.Web.Services;
using Discussion.Web.Services.TopicManagement;
using Discussion.Web.Tests.Fixtures;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class TopicControllerSpecs
    {
        private readonly TestDiscussionWebApp _app;
        private readonly User _author;

        public TopicControllerSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
            _app.DeleteAll<Topic>();
            _author = _app.CreateUser();
        }


        #region List Topics
        
        // ReSharper disable PossibleNullReferenceException
        
        [Fact]
        public void should_serve_topic_list_on_page()
        {
            _app.NewTopic("dummy topic 1").WithReply().Create();
            _app.NewTopic("dummy topic 2", type: TopicType.Question).WithReply().Create();
            _app.NewTopic("dummy topic 3", type: TopicType.Job).WithReply().Create();
            
            var topicController = _app.CreateController<TopicController>();

            var topicListResult = topicController.List() as ViewResult;
            var listViewModel = topicListResult.ViewData.Model as Paged<Topic>;

            Assert.NotNull(listViewModel);
            var showedTopics = listViewModel.Items;
            
            showedTopics.ShouldContain(t => t.Title == "dummy topic 1" && t.Type == TopicType.Discussion);
            showedTopics.ShouldContain(t => t.Title == "dummy topic 2" && t.Type == TopicType.Question);
            showedTopics.ShouldContain(t => t.Title == "dummy topic 3" && t.Type == TopicType.Job);
            
            showedTopics.All(t => t.Author != null).ShouldEqual(true);
            showedTopics.All(t => t.LastRepliedUser != null).ShouldEqual(true);
        }

        [Fact]
        public void should_calc_topic_list_with_paging()
        {
            var all = 30;
            do
            {
                // ReSharper disable once AccessToModifiedClosure
                _app.NewTopic("dummy topic " + all)
                    .WithAuthor(_author)
                    .With(t => t.CreatedAtUtc = DateTime.Today.AddSeconds(-all))
                    .Create();
            } while (--all > 0);

            var topicController = _app.CreateController<TopicController>();

            var topicListResult = topicController.List(2) as ViewResult;
            var listViewModel = topicListResult.ViewData.Model as Paged<Topic>;

            listViewModel.ShouldNotBeNull();
            listViewModel.Paging.CurrentPage.ShouldEqual(2);
            listViewModel.Paging.HasPreviousPage.ShouldEqual(true);
            listViewModel.Paging.HasNextPage.ShouldEqual(false);

            var topicList = listViewModel.Items;
            topicList.Length.ShouldEqual(10);
            topicList[0].Title.ShouldEqual("dummy topic 21");
            topicList[9].Title.ShouldEqual("dummy topic 30");
        }

        #endregion
        
        #region Create Topics
        
        [Fact]
        public void should_create_topic()
        {
            _app.MockUser();
            var topicController = _app.CreateController<TopicController>();

            var model = new TopicCreationModel()
            {
                Title = "first topic you created",
                Content = "**This is the content of this markdown**\r\n* markdown content is greate*",
                Type = TopicType.Job
            };
            topicController.CreateTopic(model);

            var repo = _app.GetService<IRepository<Topic>>();
            var allTopics = repo.All().ToList();

            var createdTopic = allTopics.Find(topic => topic.Title == model.Title);

            createdTopic.ShouldNotBeNull();
            createdTopic.Title.ShouldEqual(model.Title);
            createdTopic.Content.ShouldEqual(model.Content);
            createdTopic.Type.ShouldEqual(TopicType.Job);
            createdTopic.CreatedBy.ShouldEqual(_app.GetDiscussionUser().Id);

            var createdAt = DateTime.UtcNow - createdTopic.CreatedAtUtc;
            Assert.True(createdAt.TotalMilliseconds >= 0);
            Assert.True(createdAt.TotalMinutes < 2);

            createdTopic.LastRepliedAt.ShouldBeNull();
            createdTopic.ReplyCount.ShouldEqual(0);
            createdTopic.ViewCount.ShouldEqual(0);
        }
        
        [Fact]
        public void should_not_create_topic_if_require_verified_phone_number_but_user_has_no()
        {
            var topicRepo = new Mock<IRepository<Topic>>();
            var siteSettings = new SiteSettings {RequireUserPhoneNumberVerified = true};
            var topicController = CreateControllerWithSettings(_app.MockUser(), siteSettings, topicRepo);


            var actionResult = topicController.CreateTopic(
                new TopicCreationModel
                {
                    Title = "first topic you created",
                    Content = "some content",
                    Type = TopicType.Job
                });
            
            actionResult.IsType<BadRequestResult>();
            topicRepo.VerifyNoOtherCalls();
        }

        private TopicController CreateControllerWithSettings(User user, SiteSettings siteSettings,  Mock<IRepository<Topic>> topicRepo)
        {
            var userMock = new Mock<ICurrentUser>();
            userMock.SetupGet(u => u.DiscussionUser).Returns(user);

            var topicService = new DefaultTopicService(siteSettings, userMock.Object, topicRepo.Object, null, new SystemClock());
            var topicController = new TopicController(topicRepo.Object, topicService, NullLogger<TopicController>.Instance)
            {
                ControllerContext =
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = _app.User, RequestServices = _app.ApplicationServices
                    }
                }
            };
            return topicController;
        }

        #endregion

        #region Show Topics

        [Fact]
        public void should_show_topic_and_update_view_count()
        {
            var topic = _app.NewTopic("dummy topic 1")
                            .WithAuthor(_author)
                            .With(t => t.ViewCount = 4)
                            .Create();

            var topicController = _app.CreateController<TopicController>();
            var result = topicController.Index(topic.Id) as ViewResult;

            result.ShouldNotBeNull();

            var viewModel = result.ViewData.Model;
            var topicShown = viewModel as TopicViewModel;
            topicShown.ShouldNotBeNull();
            topicShown.Id.ShouldEqual(topic.Id);
            topicShown.Topic.Title.ShouldEqual(topic.Title);
            topicShown.Topic.Content.ShouldEqual(topic.Content);
            topicShown.Topic.ViewCount.ShouldEqual(5);
        }

        [Fact]
        public void should_show_topic_with_reply_list()
        {
            var topic = _app.NewTopic().WithAuthor(_author).WithReply("reply content 1").Create();
            var newReply = new Reply { CreatedAtUtc = DateTime.Today.AddDays(1), Content = "reply content 2", TopicId = topic.Id, CreatedBy = _author.Id };
            _app.GetService<IRepository<Reply>>().Save(newReply);
            _app.Reset();

            // Act
            var topicController = _app.CreateController<TopicController>();
            var result = topicController.Index(topic.Id) as ViewResult;

            // Assert
            result.ShouldNotBeNull();

            var viewModel = result.ViewData.Model;
            var topicShown = viewModel as TopicViewModel;
            topicShown.ShouldNotBeNull();
            topicShown.Id.ShouldEqual(topic.Id);
            topicShown.Replies.ShouldNotBeNull();

            topicShown.Replies.Count.ShouldEqual(2);
            topicShown.Replies[0].Content.ShouldEqual("reply content 1");
            topicShown.Replies[0].TopicId.ShouldEqual(topic.Id);

            topicShown.Replies[1].Content.ShouldEqual("reply content 2");
            topicShown.Replies[1].TopicId.ShouldEqual(topic.Id);
        }
        
        #endregion
    }
}