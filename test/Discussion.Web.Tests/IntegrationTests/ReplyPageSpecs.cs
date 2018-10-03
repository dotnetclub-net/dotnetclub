using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discussion.Web.Tests.Specs.Controllers;
using Xunit;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("AppSpecs")]
    public class ReplyPageSpecs
    {
        private readonly TestApplication _theApp;

        public ReplyPageSpecs(TestApplication theApp)
        {
            _theApp = theApp.Reset();
        }

        [Fact]
        public async Task should_reply_a_topic_by_an_authorized_user()
        {
            _theApp.MockUser();
            var (topic, userId) = ReplyControllerSpecs.CreateTopic(_theApp);

            var response = await RequestToCreateReply(_theApp, topic.Id);

            response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
        }


        [Fact]
        public async Task should_not_be_able_to_reply_a_topic_by_anonymous_user()
        {
            _theApp.MockUser();
            var (topic, userId) = ReplyControllerSpecs.CreateTopic(_theApp);
            
            _theApp.Reset();
            var response = await RequestToCreateReply(_theApp, topic.Id);

            response.StatusCode.ShouldEqual(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Contains("signin").ShouldEqual(true);
        }
        
        
        internal static async Task<HttpResponseMessage> RequestToCreateReply( TestApplication testApplication, int topicId, string content = null)
        {
            var request = testApplication.RequestAntiForgeryForm(
                $"/topics/{topicId}/replies",
                new Dictionary<string, string>
                {
                    {"Content", content ?? "reply content"}
                });

            return await request.PostAsync();
        }
        
    }
}