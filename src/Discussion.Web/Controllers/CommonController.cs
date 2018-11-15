using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Discussion.Core.Data;
using Discussion.Core.FileSystem;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Web.Services.Markdown;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Discussion.Web.Controllers
{
    [Authorize]
    [Route("api/common")]
    public class CommonController : ControllerBase
    {
        private readonly IFileSystem _fileSystem;
        private readonly IRepository<FileRecord> _fileRepo;
        private readonly IContentTypeProvider _contentTypeProvider;

        public CommonController(IFileSystem fileSystem, IRepository<FileRecord> fileRepo, IContentTypeProvider contentTypeProvider)
        {
            _fileSystem = fileSystem;
            _fileRepo = fileRepo;
            _contentTypeProvider = contentTypeProvider;
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
            var preventedFileNameChars = new[] { '|', '\\', '?', '*', '<', '>', ':', '"', '\''  };
            
            if (string.IsNullOrEmpty(category)
                || preventedFileNameChars.Any(category.Contains)
                || file == null || file.Length < 1)
            {
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
                Category = category
            };
            _fileRepo.Save(fileRecord);
            

            // ReSharper disable Mvc.ActionNotResolved
            // ReSharper disable Mvc.ControllerNotResolved
            var fileUrl = _fileSystem.SupportGeneratingPublicUrl 
                ? await storageFile.GetPublicUrlAsync(TimeSpan.MaxValue)
                : Url.Action("DownloadFile", "Common", new {id = fileRecord.Id}, Request.Scheme);

            return ApiResponse.ActionResult(new
            {
                FileId = fileRecord.Id,
                PublicUrl = fileUrl,
            });
        }

        [HttpGet("download/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadFile(int id)
        {
            var fileRecord = _fileRepo.Get(id);
            if (fileRecord == null)
            {
                return NotFound();
            }

            var file = await _fileSystem.GetFileAsync(fileRecord.StoragePath);
            if (file == null)
            {
                return NotFound();
            }

            if(!_contentTypeProvider.TryGetContentType(fileRecord.OriginalName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var buffered = new BufferedStream(await file.OpenReadAsync(), 8 * 1024);
            return new FileStreamResult(buffered, contentType)
            {
                FileDownloadName =  fileRecord.OriginalName
            };  
        }
    }
}