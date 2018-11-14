using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Xunit;


namespace Discussion.Web.Tests.IntegrationTests
{
    [Collection("WebSpecs")]
    public class CommonRelatedApiSpecs
    {
        private readonly TestDiscussionWebApp _app;
        public CommonRelatedApiSpecs(TestDiscussionWebApp app) {
            _app = app.Reset();
        }

        [Fact]
        public void should_serve_convert_md2html_api_correctly()
        {
            _app.Path("/api/common/md2html")
                .Post()
                .WithForm(new
                {
                    markdown = "# 中文的 title"
                })
                .ShouldSuccess(_app.MockUser())
                .WithApiResult((api, result) => 
                    result["html"].ToString()
                    .ShouldContain("<h2>中文的 title</h2>"))
                .And
                .ShouldFail(_app.NoUser())
                .WithSigninRedirect();
        }
        
        [Fact]
        public void should_upload_file_by_authorized_user()
        {
            // todo: add integration test for uploading a file
            
        }
        [Fact]
        public void should_download_file_by_anonymous_user()
        {
            // todo: add integration test for uploading a file
            
        }
    }
}