using System;
using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Pagination;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Controllers;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class TopicControllerSpecs
    {
        private readonly TestDiscussionWebApp _app;

        public TopicControllerSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
        }

        [Fact]
        public void should_serve_topic_list_on_page()
        {
            var topicItems = new[]
            {
                new Topic {Title = "dummy topic 1", Type = TopicType.Discussion},
                new Topic {Title = "dummy topic 2", Type = TopicType.Discussion },
                new Topic {Title = "dummy topic 3", Type = TopicType.Discussion },
            };
            var repo = _app.GetService<IRepository<Topic>>();
            foreach (var item in topicItems)
            {
                repo.Save(item);
            }

            var topicController = _app.CreateController<TopicController>();

            var topicListResult = topicController.List() as ViewResult;
            var listViewModel = topicListResult.ViewData.Model as Paged<Topic>;

            listViewModel.ShouldNotBeNull();
            listViewModel.Items.ShouldContain(t => t.Title == "dummy topic 1");
            listViewModel.Items.ShouldContain(t => t.Title == "dummy topic 2");
            listViewModel.Items.ShouldContain(t => t.Title == "dummy topic 3");
        }

        [Fact]
        public void should_calc_topic_list_with_paging()
        {
            _app.DeleteAll<Topic>();
            var repo = _app.GetService<IRepository<Topic>>();
            var all = 30;
            do
            {
                repo.Save(new Topic
                {
                    Title = "dummy topic " + all,
                    Type = TopicType.Discussion,
                    CreatedAtUtc = DateTime.Today.AddSeconds(-all)
                });
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
        public void should_show_topic_and_update_view_count()
        {
            _app.MockUser();
            var topic = new Topic
            {
                Title = "dummy topic 1",
                ViewCount = 4,
                Type = TopicType.Discussion,
                CreatedBy = _app.User.ExtractUserId().Value
            };
            var repo = _app.GetService<IRepository<Topic>>();
            repo.Save(topic);
            _app.Reset();

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
            _app.MockUser();
            // Arrange
            var topicRepo = _app.GetService<IRepository<Topic>>();
            var replyRepo = _app.GetService<IRepository<Reply>>();

            var topic = new Topic { Title = "dummy topic 1", Type = TopicType.Discussion, CreatedBy = _app.User.ExtractUserId().Value };
            topicRepo.Save(topic);

            var replyContent = "reply content ";
            var replyNew = new Reply { CreatedAtUtc = DateTime.Today.AddDays(1), Content = replyContent + "2", TopicId = topic.Id, CreatedBy = _app.User.ExtractUserId().Value };
            var replyOld = new Reply { CreatedAtUtc = DateTime.Today.AddDays(-1), Content = replyContent + "1", TopicId = topic.Id, CreatedBy = _app.User.ExtractUserId().Value };
            replyRepo.Save(replyNew);
            replyRepo.Save(replyOld);
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
            topicShown.Replies[0].Content.ShouldEqual(replyContent + "1");
            topicShown.Replies[0].TopicId.ShouldEqual(topic.Id);

            topicShown.Replies[1].Content.ShouldEqual(replyContent + "2");
            topicShown.Replies[1].TopicId.ShouldEqual(topic.Id);
        }
    }
}