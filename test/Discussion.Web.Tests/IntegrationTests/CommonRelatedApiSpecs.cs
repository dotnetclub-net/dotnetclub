using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.FileSystem;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
        public async Task should_upload_file_by_authorized_user()
        {
            _app.MockUser();
            var tokens = _app.GetAntiForgeryTokens();
            var request = _app.Server.CreateRequest("/api/common/upload/avatar");

            var multipart = new MultipartFormDataContent("------------------------" + StringUtility.Random(8));
            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("hello file"));
            fileContent.Headers.Add("Content-Type", "application/octet-stream");
            multipart.Add(fileContent, "file", "filename.txt");
            multipart.Add(new StringContent(tokens.VerificationToken), "__RequestVerificationToken");

            var response = await request
                .And(req => req.Content = multipart)
                .WithCookie(tokens.Cookie)
                .PostAsync();
            
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("publicUrl", response.ReadAllContent());
        }
        
        [Fact]
        public void should_download_file_by_anonymous_user()
        {
            var mockFile = new Mock<IFile>();
            mockFile.Setup(f => f.OpenReadAsync()).Returns(Task.FromResult((Stream)new MemoryStream()));
            
            var mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.GetFileAsync("file-path")).Returns(Task.FromResult(mockFile.Object));
            
            var mockRepo = new Mock<IRepository<FileRecord>>();
            mockRepo.Setup(repo => repo.Get(42)).Returns(new FileRecord {StoragePath = "file-path", OriginalName = "file.txt"});
            
            _app.OverrideServices(services =>
            {
                services.AddSingleton(mockFileSystem.Object);
                services.AddSingleton(mockRepo.Object);
            });
            
            
            _app.Path("/api/common/download/42")
                .Get()
                .ShouldSuccess(_app.NoUser())
                .WithResponse(res =>
                {
                    res.Content.Headers.ContentType.MediaType.ShouldEqual("text/plain");
                    res.Content.Headers.ContentDisposition.FileName.ShouldEqual("file.txt");
                });
        }
        
        
    }
}