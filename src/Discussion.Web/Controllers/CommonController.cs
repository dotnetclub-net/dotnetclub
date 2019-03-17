using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discussion.Core.Data;
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

        public CommonController(IFileSystem fileSystem, IRepository<FileRecord> fileRepo, IContentTypeProvider contentTypeProvider, ILogger<CommonController> logger)
        {
            _fileSystem = fileSystem;
            _fileRepo = fileRepo;
            _contentTypeProvider = contentTypeProvider;
            _logger = logger;
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
        //[ResponseCache(Duration = 600)]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFile(string slug, [FromQuery] bool? download)
        {
            //根据etag 头 和expires 头判断是否要返回缓存
            //返回缓存条件  expires没过期&etag文件没有发生变化
            //重新请求条件 ① expires过期 ②expires没过期，但是etag发生变化 ③没有expires 或者etag头
            var entityTag = new EntityTagHeaderValue("\"CalculatedEtagValue\"");
            string dt = Request.Headers["If-Modified-Since"];
            DateTime isModifiedSince;
            DateTime.TryParse(dt, out isModifiedSince);
            string etag= Request.Headers["If-None-Match"];
            //if(!string.IsNullOrWhiteSpace(etag))
            //    etag = etag.Replace("\"", "");
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
            var buffered = new BufferedStream(await file.OpenReadAsync(), 8 * 1024);
            var shouldDownload = download.HasValue && download.Value;
            long length;
            length = file.GetSize();
            var last = fileRecord.ModifiedAtUtc;
            DateTimeOffset lastOffset = DateTime.SpecifyKind(last, DateTimeKind.Utc);
            var _lastModified = new DateTimeOffset(lastOffset.Year, lastOffset.Month, lastOffset.Day,
                            lastOffset.Hour, lastOffset.Minute, lastOffset.Second, lastOffset.Offset).ToUniversalTime();
            long etagHash = _lastModified.ToFileTime() ^ length;
            entityTag = new EntityTagHeaderValue('\"' + Convert.ToString(etagHash, 16) + '\"');
            if (isModifiedSince != null && isModifiedSince>= DateTime.Now && entityTag.Tag.Value.Equals(etag))
            {
                return new StatusCodeResult(304);  
            }
            else
            {
                if (!_contentTypeProvider.TryGetContentType(fileRecord.OriginalName, out var contentType))
                {
                    contentType = "aplpication/octet-stream";
                }
                return new FileStreamResult(buffered, contentType)
                {
                    FileDownloadName = shouldDownload ? fileRecord.OriginalName : null,
                    EntityTag = entityTag,
                    LastModified = DateTime.UtcNow.AddDays(1)
                };
            }

            #region Old Return Response
            //return File(buffered, "text/plain", "downloadName.txt",
            //lastModified: DateTime.UtcNow.AddSeconds(-5), entityTag: entityTag);
            #endregion
        }
    }
}