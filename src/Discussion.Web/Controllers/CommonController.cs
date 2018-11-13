using System;
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

namespace Discussion.Web.Controllers
{
    [Authorize]
    [Route("api/common")]
    public class CommonController : ControllerBase
    {
        private readonly IFileSystem _fileSystem;
        private readonly IRepository<FileRecord> _fileRepo;

        public CommonController(IFileSystem fileSystem, IRepository<FileRecord> fileRepo)
        {
            _fileSystem = fileSystem;
            _fileRepo = fileRepo;
        }
        
        [HttpPost("md2html")]
        public object RenderMarkdown([FromForm]string markdown, int maxHeadingLevel = 2)
        {
            var htmlString = string.IsNullOrWhiteSpace(markdown)
                 ? markdown
                 : markdown.MdToHtml(maxHeadingLevel);

            return new { html = htmlString };
        }
        
        
        [HttpPost("upload/{category}")]
        public async Task<ApiResponse> UploadFile(IFormFile file, string category)
        {
            var preventedFileNameChars = new[] { '|', '\\', '/', '?', '*', '<', '>', ':', '"', '\''  };
            
            if (string.IsNullOrEmpty(category)
                || preventedFileNameChars.Any(category.Contains)
                || file == null || file.Length < 1)
            {
                return ApiResponse.NoContent(HttpStatusCode.BadRequest);
            }
            
            var subPath = string.Concat(category,
                _fileSystem.GetDirectorySeparatorChar(),
                Guid.NewGuid().ToString("N"));

            var fileRecord = new FileRecord
            {
                UploadedBy = HttpContext.DiscussionUser().Id,
                Size = file.Length,
                OriginalName = file.FileName,
                StoragePath = subPath
            };
            _fileRepo.Save(fileRecord);
            
            var storageFile = await _fileSystem.CreateFileAsync(subPath);
            var outputStream = await storageFile.OpenWriteAsync();
            await file.CopyToAsync(outputStream);
            
            return ApiResponse.ActionResult(new { fileId = fileRecord.Id });
        }
    }
}