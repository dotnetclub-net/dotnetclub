using Discussion.Tests.Common;
using Discussion.Web.Tests.Fixtures;
using Xunit;
using static Discussion.Tests.Common.SigninRequirement;

namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("WebSpecs")]
    public class ReplyPageSpecs
    {
        private readonly TestDiscussionWebApp _app;

        public ReplyPageSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
        }

        [Fact]
        public void should_reply_a_topic_by_an_authorized_user()
        {
            var topic = _app.NewTopic().Create();

            _app.ShouldPost($"/topics/{topic.Id}/replies",
                new
                {
                    Content = "reply content"
                },
                SigninRequired);
        }       
    }
}