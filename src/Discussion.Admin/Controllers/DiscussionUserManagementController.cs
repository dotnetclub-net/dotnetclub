using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Discussion.Admin.ViewModels;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Models.UserAvatar;
using Discussion.Core.Mvc;
using Discussion.Core.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Discussion.Admin.Controllers
{
    [Authorize]
    public class DiscussionUserManagementController: ControllerBase, IDisposable
    {
        const int PageSize = 20;
        private readonly IRepository<User> _userRepo;
        private readonly SiteSettings _settings;
        private readonly MD5 _md5;

        public DiscussionUserManagementController(IRepository<User> userRepo, IRepository<SiteSettings> settingsRepo)
        {
            _md5 = MD5.Create();
            _userRepo = userRepo;
            _settings = settingsRepo.All().FirstOrDefault() ?? new SiteSettings();
        }

        [Route("api/discussion-users")]
        public Paged<UserSummary> List(int? page = 1)
        {
            return _userRepo.All()
                .OrderByDescending(user => user.Id)
                    .Include(user => user.AvatarFile)
                .Page(SummarizeUser(), PageSize, page);
        }

        [Route("api/discussion-users/{id}")]
        [HttpGet]
        public ApiResponse ShowDetail(int id)
        {
            throw new System.NotImplementedException();
        }
        
        [Route("api/discussion-users/{id}/block")]
        [HttpPost]
        public ApiResponse Block(int id, [FromQuery] int days)
        {
            throw new System.NotImplementedException();
        }

        [Route("api/discussion-users/{id}/unblock")]
        [HttpDelete]
        public ApiResponse UnBlock(int id)
        {
            throw new System.NotImplementedException();
        }

        private Func<User,UserSummary> SummarizeUser()
        {
            return user => new UserSummary
            {
                Id = user.Id,
                LoginName = user.UserName,
                CreatedAt = user.CreatedAtUtc,
                DisplayName = user.DisplayName,
                AvatarUrl = GetUserAvatarUrl(user)
            };
        }



        public string GetUserAvatarUrl(User user)
        {
            if (user == null)
            {
                return null;
            }

            var avatar = user.GetAvatar();
            if (avatar is StorageFileAvatar fileAvatar)
            {
                return $"https://{_settings.PublicHostName}/api/common/download/{fileAvatar.StorageFileSlug}";
            }

            if (avatar is GravatarAvatar gravatarAvatar)
            {
                var hash = Md5Hash(gravatarAvatar.EmailAddress);
                return $"https://www.gravatar.com/avatar/{hash}?size=160&d=robohash";
            }

            return $"https://{_settings.PublicHostName}/assets/default-avatar.jpg";
        }


        private string Md5Hash(string emailAddress)
        {
            var bytes = _md5.ComputeHash(Encoding.ASCII.GetBytes(emailAddress));
            return bytes
                .Select(x => x.ToString("x2"))
                .Aggregate(string.Concat);
        }

        void IDisposable.Dispose()
        {
            _md5.Dispose();
        }
    }
}