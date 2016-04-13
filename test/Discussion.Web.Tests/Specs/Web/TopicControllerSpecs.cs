using Discussion.Web.Controllers;
using Discussion.Web.Models;
using Discussion.Web.Data;
using Microsoft.AspNet.Mvc;
using System.Collections.Generic;
using Xunit;

namespace Discussion.Web.Tests.Specs.Web
{

    [Collection("AppSpecs")]
    public class TopicControllerSpecs
    {
        public Application _myApp;
        public TopicControllerSpecs(Application app)
        {
            _myApp = app;
        }


        [Fact]
        public void should_serve_topic_list_page_as_view_result()
        {
            var topicController = _myApp.CreateController<TopicController>();

            var topicListResult = topicController.List();

            topicListResult.ShouldNotBeNull();
            topicListResult.IsType<ViewResult>();
        }



        [Fact]
        public void should_serve_topic_list_on_page()
        {
            var topicItems = new[]
            {
                new Topic {Title = "dummy topic 1" },
                new Topic {Title = "dummy topic 2" },
                new Topic {Title = "dummy topic 3" },
            };
            var repo = _myApp.GetService<IDataRepository<Topic>>();
            foreach(var item in topicItems)
            {
                repo.Create(item);
            }


            var topicController = _myApp.CreateController<TopicController>();

            var topicListResult = topicController.List() as ViewResult;
            var topicList = topicListResult.ViewData.Model as IList<Topic>;

            topicList.ShouldNotBeNull();
            topicList.Count.ShouldEqual(3);
            topicList.ShouldContain(t => t.Title == "dummy topic 1");
            topicList.ShouldContain(t => t.Title == "dummy topic 2");
            topicList.ShouldContain(t => t.Title == "dummy topic 3");
        }


    }
}
