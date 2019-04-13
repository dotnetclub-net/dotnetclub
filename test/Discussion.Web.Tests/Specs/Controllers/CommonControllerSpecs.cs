using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.ETag;
using Discussion.Core.FileSystem;
using Discussion.Core.Models;
using Discussion.Core.Utilities;
using Discussion.Tests.Common;
using Discussion.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class CommonControllerSpecs
    {
        private readonly TestDiscussionWebApp _app;
        private readonly IRepository<FileRecord> _fileRepo;
        private readonly IFileSystem _fs;
        private readonly ITagBuilder _tagBuilder;


        public CommonControllerSpecs(TestDiscussionWebApp app)
        {
            _app = app.Reset();
            _fileRepo = _app.GetService<IRepository<FileRecord>>();
            _fs = _app.GetService<IFileSystem>();
            _tagBuilder = _app.GetService<ITagBuilder>();
            
            _app.DeleteAll<FileRecord>();
        }

        [Fact]
        public void convert_markdown_to_html()
        {
            // Act
            var commonController = _app.CreateController<CommonController>();

            // Action
            dynamic htmlFromMd = commonController.RenderMarkdown("# Title");

            Assert.Equal("<h2>Title</h2>\n", htmlFromMd.Html);
        }

        [Fact]
        public async Task should_upload_file()
        {
            _app.MockUser();
            var file = MockFile("test-file.txt");

            var commonController = _app.CreateController<CommonController>();
            commonController.Url = CreateMockUrlHelper("file-link");
            var requestResult = await commonController.UploadFile(file, "avatar");

            Assert.NotNull(requestResult);
            Assert.True(requestResult.HasSucceeded);

            dynamic uploadResult = requestResult.Result;
            int fileId = uploadResult.FileId;
            var fileRecord = _fileRepo.Get(fileId);
            Assert.Equal("avatar", fileRecord.Category);
            Assert.Equal("test-file.txt", fileRecord.OriginalName);

            string publicUrl = uploadResult.PublicUrl;
            Assert.Equal("file-link", publicUrl);
        }

        [Fact]
        public async Task should_download_file()
        {
            const string fileContent = "Hello World from a Fake File 其中还包含中文";

            var fileRecord = await CreateUploadedFileAsync(fileContent);

            var commonController = _app.CreateController<CommonController>();
            var downloadResult = await commonController.DownloadFile(fileRecord.Slug, download: true) as FileStreamResult;

            Assert.NotNull(downloadResult);
            Assert.Equal(fileRecord.OriginalName, downloadResult.FileDownloadName);
            Assert.Equal(fileRecord.Size, downloadResult.FileStream.Length);
            using (var reader = new StreamReader(downloadResult.FileStream))
            {
                var content = await reader.ReadToEndAsync();
                Assert.Equal(fileContent, content);
            }
        }

        private async Task<FileRecord> CreateUploadedFileAsync(string fileContent)
        {
            var rand = StringUtility.Random(5);
            var storageFile = await _fs.CreateFileAsync($"testing/the-file-{rand}.txt");
            using (var ms = new MemoryStream())
            {
                var writer = new StreamWriter(ms);
                writer.Write(fileContent);
                writer.Flush();
                ms.Seek(0, SeekOrigin.Begin);

                using (var dest = await storageFile.OpenWriteAsync())
                {
                    await ms.CopyToAsync(dest);
                }
            }

            var fileRecord = new FileRecord
            {
                OriginalName = "the-file.txt",
                Category = "testing",
                UploadedBy = _app.MockUser().Id,
                StoragePath = storageFile.GetPath(),
                Size = storageFile.GetSize(),
                Slug = "be8f02ca8fd44d0dbbc76513b6221a9f",
                ModifiedAtUtc = new DateTime(2019, 04, 12)
            };
            _fileRepo.Save(fileRecord);
            return fileRecord;
        }

        private static IFormFile MockFile(string fileName)
        {
            var fileMock = new Mock<IFormFile>();
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write("Hello World from a Fake File");
            writer.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
            return fileMock.Object;
        }

        private static IUrlHelper CreateMockUrlHelper(string fileLink)
        {
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(url => url.Action(It.IsAny<UrlActionContext>())).Returns(fileLink);
            return urlHelper.Object;
        }

        [Fact]
        public async Task should_down_file_server()
        {
            var file = await CreateUploadedFileAsync("some content");

            var commonController = _app.CreateController<CommonController>();
            var downloadResult = await commonController.DownloadFile(file.Slug, download: false) as FileStreamResult;

            Assert.NotNull(downloadResult);
            Assert.Equal("\"1d4f0c2abf7000c\"", downloadResult.EntityTag.Tag.Value);
        }   
        
       
        [Fact]
        public async Task should_down_file_server_with_if_non_match_etag()
        {
            var file = await CreateUploadedFileAsync("some content");

            var commonController = _app.CreateController<CommonController>();
            commonController.Request.Headers.Add("If-None-Match", "\"1d4f0c2abf7000c\"");
            var downloadResult = await commonController.DownloadFile(file.Slug, download: false) as StatusCodeResult;
            
            Assert.NotNull(downloadResult);
            Assert.Equal(304, downloadResult.StatusCode);
        }
        
        [Fact]
        public async Task should_down_file_server_with_if_modified_since()
        {
            var file = await CreateUploadedFileAsync("some content");

            var commonController = _app.CreateController<CommonController>();
            commonController.Request.Headers["If-Modified-Since"] = new DateTime(2019, 4, 13).ToString("R");
            var downloadResult = await commonController.DownloadFile(file.Slug, download: false) as StatusCodeResult;
            
            Assert.NotNull(downloadResult);
            Assert.Equal(304, downloadResult.StatusCode);
        }


        [Fact]
        public void should_build_etag()
       {
            var fileModifiedAtUtc = DateTime.Parse("2002/2/13 0:00:00");
            
            var etag = _tagBuilder.EntityTagBuild(fileModifiedAtUtc,4166);
            
            Assert.Equal("\"1c1b4216127d046\"", etag.Tag.ToString());
        }
    }
}