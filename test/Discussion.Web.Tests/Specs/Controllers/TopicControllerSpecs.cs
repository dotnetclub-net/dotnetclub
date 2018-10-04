using System;
using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Web.Controllers;
using Discussion.Web.Services.Identity;
using Discussion.Web.Services.Markdown;
using Discussion.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("AppSpecs")]
    public class TopicControllerSpecs
    {
        public TestApplication _myApp;

        public TopicControllerSpecs(TestApplication app)
        {
            _myApp = app.Reset();
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
            var repo = _myApp.GetService<IRepository<Topic>>();
            foreach (var item in topicItems)
            {
                repo.Save(item);
            }

            var topicController = _myApp.CreateController<TopicController>();

            var topicListResult = topicController.List() as ViewResult;
            var listViewModel = topicListResult.ViewData.Model as TopicListViewModel;

            listViewModel.ShouldNotBeNull();
            var topicList = listViewModel.Topics;
            topicList.ShouldContain(t => t.Title == "dummy topic 1");
            topicList.ShouldContain(t => t.Title == "dummy topic 2");
            topicList.ShouldContain(t => t.Title == "dummy topic 3");
        }

        [Fact]
        public void should_calc_topic_list_with_paging()
        {
            var repo = _myApp.GetService<IRepository<Topic>>();
            repo.All().ToList().ForEach(topic => repo.Delete(topic));
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

            var topicController = _myApp.CreateController<TopicController>();

            var topicListResult = topicController.List(2) as ViewResult;
            var listViewModel = topicListResult.ViewData.Model as TopicListViewModel;

            listViewModel.ShouldNotBeNull();
            listViewModel.CurrentPage.ShouldEqual(2);
            listViewModel.HasPreviousPage.ShouldEqual(true);
            listViewModel.HasNextPage.ShouldEqual(false);

            var topicList = listViewModel.Topics;
            topicList.Count.ShouldEqual(10);
            topicList[0].Title.ShouldEqual("dummy topic 21");
            topicList[9].Title.ShouldEqual("dummy topic 30");
        }

        [Fact]
        public void should_create_topic()
        {
            _myApp.MockUser();
            var topicController = _myApp.CreateController<TopicController>();

            var model = new TopicCreationModel()
            {
                Title = "first topic you created",
                Content = "**This is the content of this markdown**\r\n* markdown content is greate*",
                Type = TopicType.Job
            };
            topicController.CreateTopic(model);

            var repo = _myApp.GetService<IRepository<Topic>>();
            var allTopics = repo.All().ToList();

            var createdTopic = allTopics.Find(topic => topic.Title == model.Title);

            createdTopic.ShouldNotBeNull();
            createdTopic.Title.ShouldEqual(model.Title);
            createdTopic.Content.ShouldEqual(model.Content);
            createdTopic.Type.ShouldEqual(TopicType.Job);
            createdTopic.CreatedBy.ShouldEqual(_myApp.GetDiscussionUser().Id);

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
            _myApp.MockUser();
            var topic = new Topic
            {
                Title = "dummy topic 1",
                ViewCount = 4,
                Type = TopicType.Discussion,
                CreatedBy = _myApp.User.ExtractUserId().Value
            };
            var repo = _myApp.GetService<IRepository<Topic>>();
            repo.Save(topic);
            _myApp.Reset();

            var topicController = _myApp.CreateController<TopicController>();
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
            _myApp.MockUser();
            // Arrange
            var topicRepo = _myApp.GetService<IRepository<Topic>>();
            var replyRepo = _myApp.GetService<IRepository<Reply>>();

            var topic = new Topic { Title = "dummy topic 1", Type = TopicType.Discussion, CreatedBy = _myApp.User.ExtractUserId().Value };
            topicRepo.Save(topic);

            var replyContent = "reply content ";
            var replyNew = new Reply { CreatedAtUtc = DateTime.Today.AddDays(1), Content = replyContent + "2", TopicId = topic.Id, CreatedBy = _myApp.User.ExtractUserId().Value };
            var replyOld = new Reply { CreatedAtUtc = DateTime.Today.AddDays(-1), Content = replyContent + "1", TopicId = topic.Id, CreatedBy = _myApp.User.ExtractUserId().Value };
            replyRepo.Save(replyNew);
            replyRepo.Save(replyOld);
            _myApp.Reset();

            // Act
            var topicController = _myApp.CreateController<TopicController>();
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