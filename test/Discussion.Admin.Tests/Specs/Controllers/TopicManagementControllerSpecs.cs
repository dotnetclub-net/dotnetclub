using System.Linq;
using Discussion.Admin.Controllers;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Tests.Common;
using Xunit;

namespace Discussion.Admin.Tests.Specs.Controllers
{
    [Collection("AdminSpecs")]
    public class TopicManagementControllerSpecs
    {

        private readonly TestDiscussionAdminApp _adminApp;

        public TopicManagementControllerSpecs(TestDiscussionAdminApp adminApp)
        {
            _adminApp = adminApp;
            _adminApp.DeleteAll<Topic>();
        }

        [Fact]
        public void should_show_topic_list()
        {
            var user = new User {UserName = "Jim"};
            _adminApp.GetService<IRepository<User>>().Save(user);
            
            var topicRepository = _adminApp.GetService<IRepository<Topic>>();
            Enumerable.Range(1, 3)
                .Select(num => $"title {num}")
                .ToList()
                .ForEach(title =>
                {
                    topicRepository.Save(new Topic
                    {
                        Author = user,
                        Type = TopicType.Discussion,
                        Title = title
                    });
                });

            var controller = _adminApp.CreateController<TopicManagementController>();

            var list = controller.List();
            
            Assert.Equal(3, list.TotalItemCount);
            Assert.Equal("title 3", list.Items[0].Title);
            Assert.Equal("title 2", list.Items[1].Title);
            Assert.Equal("title 1", list.Items[2].Title);
        }
        
        
        [Fact]
        public void should_delete_topic_()
        {
            var user = new User {UserName = "Jim"};
            _adminApp.GetService<IRepository<User>>().Save(user);
            
            var topicRepository = _adminApp.GetService<IRepository<Topic>>();
            var topic = new Topic
            {
                Author = user,
                Type = TopicType.Discussion,
                Title = "some title"
            };
            topicRepository.Save(topic);

            var controller = _adminApp.CreateController<TopicManagementController>();

            controller.Delete(topic.Id);

            var dbContext = _adminApp.GetService<ApplicationDbContext>();
            dbContext.Entry(topic).Reload();
            var deletedItem = topicRepository.Get(topic.Id);
            Assert.Null(deletedItem);
        }
        
        
        
        
        
    }
}