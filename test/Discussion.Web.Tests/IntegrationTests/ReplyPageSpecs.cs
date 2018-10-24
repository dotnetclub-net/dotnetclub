using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Discussion.Web.Tests.Specs.Controllers;
using Xunit;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("AppSpecs")]
    public class ReplyPageSpecs
    {
        private readonly TestDiscussionWebApp _app;

        public ReplyPageSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
        }

        [Fact]
        public async Task should_reply_a_topic_by_an_authorized_user()
        {
            _app.MockUser();
            var (topic, userId) = ReplyControllerSpecs.CreateTopic(_app);

            var response = await RequestToCreateReply(_app, topic.Id);

            response.StatusCode.ShouldEqual(HttpStatusCode.NoContent);
        }


        [Fact]
        public async Task should_not_be_able_to_reply_a_topic_by_anonymous_user()
        {
            _app.MockUser();
            var (topic, userId) = ReplyControllerSpecs.CreateTopic(_app);
            
            _app.Reset();
            var response = await RequestToCreateReply(_app, topic.Id);

            response.StatusCode.ShouldEqual(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Contains("signin").ShouldEqual(true);
        }
        
        
        internal static async Task<HttpResponseMessage> RequestToCreateReply( TestDiscussionWebApp testDiscussionWeb, int topicId, string content = null)
        {
            var request = testDiscussionWeb.RequestAntiForgeryForm(
                $"/topics/{topicId}/replies",
                new Dictionary<string, string>
                {
                    {"Content", content ?? "reply content"}
                });

            return await request.PostAsync();
        }
        
    }
}