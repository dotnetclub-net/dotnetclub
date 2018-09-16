using Discussion.Web.Controllers;
using Discussion.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Xunit;
using Discussion.Web.ViewModels;
using System.Linq;
using System;
using Jusfr.Persistent;

namespace Discussion.Web.Tests.Specs
{

    [Collection("AppSpecs")]
    public class TopicControllerSpecs
    {
        public Application _myApp;
        public TopicControllerSpecs(Application app)
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
            foreach(var item in topicItems)
            {
                repo.Create(item);
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
            repo.All.ToList().ForEach(topic => repo.Delete(topic));
            var all = 30;
            do
            {
                repo.Create(new Topic
                {
                    Title = "dummy topic " + all, 
                    Type = TopicType.Discussion, 
                    CreatedAt = DateTime.Today.AddSeconds(-all)
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
            var allTopics = repo.All.ToList();

            var createdTopic = allTopics.Find(topic => topic.Title == model.Title);

            createdTopic.ShouldNotBeNull();
            createdTopic.Title.ShouldEqual(model.Title);
            createdTopic.Content.ShouldEqual(model.Content);
            createdTopic.Type.ShouldEqual(TopicType.Job);
            createdTopic.CreatedBy.ShouldEqual((_myApp.User as DiscussionPrincipal).User.Id);

            var createdAt = DateTime.UtcNow - createdTopic.CreatedAt;
            Assert.True(createdAt.TotalMilliseconds >= 0);
            Assert.True(createdAt.TotalMinutes < 2);

            createdTopic.LastRepliedAt.ShouldBeNull();
            createdTopic.ReplyCount.ShouldEqual(0);
            createdTopic.ViewCount.ShouldEqual(0);
        }

        [Fact]
        public void should_show_topic()
        {
            var topic = new Topic { Title = "dummy topic 1", Type = TopicType.Discussion };
            var repo = _myApp.GetService<IRepository<Topic>>();
            repo.Create(topic);


            var topicController = _myApp.CreateController<TopicController>();
            var result = topicController.Index(topic.Id) as ViewResult;


            result.ShouldNotBeNull();

            var viewModel = result.ViewData.Model;
            var topicShown = viewModel as TopicShowModel;
            topicShown.ShouldNotBeNull();
            topicShown.Id.ShouldEqual(topic.Id);
        }


        [Fact]
        public void should_show_render_topic_content_from_markdown()
        {
            var topic = new Topic
            {
                Title = "dummy topic 1",
                Type = TopicType.Discussion,
                Content = @"标题哈
###哈呵呵
**功能**是*很好*的"
            };
            var repo = _myApp.GetService<IRepository<Topic>>();
            repo.Create(topic);


            var topicController = _myApp.CreateController<TopicController>();
            var result = topicController.Index(topic.Id) as ViewResult;

            result.ShouldNotBeNull();

            var viewModel = result.ViewData.Model;
            var topicShown = viewModel as TopicShowModel;
            topicShown.ShouldNotBeNull();
            topicShown.Id.ShouldEqual(topic.Id);
            topicShown.Title.ShouldEqual(topic.Title);
            topicShown.MarkdownContent.ShouldEqual(topic.Content);
            topicShown.HtmlContent.ShouldEqual("<p>标题哈</p>\n\n<h3>哈呵呵</h3>\n\n<p><strong>功能</strong>是<em>很好</em>的</p>");
        }
    }
}
