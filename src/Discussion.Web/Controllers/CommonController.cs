using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.ETag;
using Discussion.Core.FileSystem;
using Discussion.Core.Markdown;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Discussion.Web.Controllers
{
    [Authorize]
    [Route("api/common")]
    public class CommonController : ControllerBase
    {
        private readonly IFileSystem _fileSystem;
        private readonly IRepository<FileRecord> _fileRepo;
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly ILogger<CommonController> _logger;
        private readonly ITagBuilder _tagbuilder;

        public CommonController(IFileSystem fileSystem, IRepository<FileRecord> fileRepo, IContentTypeProvider contentTypeProvider, ITagBuilder tagbuilder, ILogger<CommonController> logger)
        {
            _fileSystem = fileSystem;
            _fileRepo = fileRepo;
            _contentTypeProvider = contentTypeProvider;
            _logger = logger;
            _tagbuilder = tagbuilder;
        }

        [HttpPost("md2html")]
        public object RenderMarkdown([FromForm]string markdown, int maxHeadingLevel = 2)
        {
            var htmlString = string.IsNullOrWhiteSpace(markdown)
                 ? markdown
                 : markdown.MdToHtml(maxHeadingLevel);

            return new { Html = htmlString };
        }


        [HttpPost("upload/{category}")]
        public async Task<ApiResponse> UploadFile(IFormFile file, string category)
        {
            var preventedFileNameChars = new[] { '|', '\\', '?', '*', '<', '>', ':', '"', '\'' };

            if (string.IsNullOrEmpty(category)
                || preventedFileNameChars.Any(category.Contains)
                || file == null || file.Length < 1)
            {
                _logger.LogWarning("上传文件失败：空文件，或不正确的参数");
                return ApiResponse.NoContent(HttpStatusCode.BadRequest);
            }

            category = category.Replace("/", _fileSystem.GetDirectorySeparatorChar());
            var subPath = string.Concat(category,
                _fileSystem.GetDirectorySeparatorChar(),
                Guid.NewGuid().ToString("N"));

            var storageFile = await _fileSystem.CreateFileAsync(subPath);
            using (var outputStream = await storageFile.OpenWriteAsync())
            {
                await file.CopyToAsync(outputStream);
            }


            var fileRecord = new FileRecord
            {
                UploadedBy = HttpContext.DiscussionUser().Id,
                Size = file.Length,
                OriginalName = file.FileName,
                StoragePath = subPath,
                Category = category,
                Slug = Guid.NewGuid().ToString("N")
            };
            _fileRepo.Save(fileRecord);


            // ReSharper disable Mvc.ActionNotResolved
            // ReSharper disable Mvc.ControllerNotResolved
            var fileUrl = _fileSystem.SupportGeneratingPublicUrl
                ? await storageFile.GetPublicUrlAsync(TimeSpan.MaxValue)
                : Url.Action("DownloadFile", "Common", new { slug = fileRecord.Slug }, Request.Scheme);

            _logger.LogInformation($"上传文件成功：{fileRecord.OriginalName}, {fileRecord.Size} bytes, {fileRecord.StoragePath}, (id: {fileRecord.Id})");
            return ApiResponse.ActionResult(new
            {
                FileId = fileRecord.Id,
                PublicUrl = fileUrl,
            });
        }

        [HttpGet("download/{slug}", Name = "")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFile(string slug, [FromQuery] bool? download)
        {
            var fileRecord = _fileRepo.All().FirstOrDefault(f => f.Slug.ToLower() == slug.ToLower());
            if (fileRecord == null)
            {
                return NotFound();
            }
            var file = await _fileSystem.GetFileAsync(fileRecord.StoragePath);
            if (file == null)
            {
                return NotFound();
            }
            
            var shouldDownload = download.HasValue && download.Value;
            var entityTag = _tagbuilder.EntityTagBuild(fileRecord.ModifiedAtUtc, file.GetSize()); 
            if (!shouldDownload)
            {
                var modifiedSinceHeader = Request.Headers["If-Modified-Since"];
                var modifiedSinceUtc = DateTimeOffset.MinValue; 
                var formatValid = modifiedSinceHeader.Any() 
                                  && DateTimeOffset.TryParseExact(modifiedSinceHeader.ToString(), "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out modifiedSinceUtc);
                if (formatValid && modifiedSinceUtc.UtcDateTime >= fileRecord.ModifiedAtUtc)
                {
                    return new StatusCodeResult(304);
                }
                
                var etagHeader = Request.Headers["If-None-Match"];
                if (etagHeader.Any() && entityTag.Tag.Value.Trim('\"').Equals(etagHeader.ToString().Trim('\"')))
                {
                    return new StatusCodeResult(304);
                }
            }
            
            var buffered = new BufferedStream(await file.OpenReadAsync(), 8 * 1024);
            if (!_contentTypeProvider.TryGetContentType(fileRecord.OriginalName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return new FileStreamResult(buffered, contentType)
            {
                FileDownloadName = shouldDownload ? fileRecord.OriginalName : null,
                EntityTag = entityTag,
                LastModified = DateTime.UtcNow.AddDays(1)
            };
        }
    }
}