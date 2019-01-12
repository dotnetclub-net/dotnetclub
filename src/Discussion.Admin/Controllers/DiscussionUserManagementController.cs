using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Discussion.Admin.ViewModels;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Core.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discussion.Admin.Controllers
{
    [Authorize]
    public class DiscussionUserManagementController: ControllerBase
    {
        const int PageSize = 20;
        private readonly IRepository<User> _userRepo;

        public DiscussionUserManagementController(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        [Route("api/discussion-users")]
        public Paged<UserSummary> List(int? page = 1)
        {
            return _userRepo.All()
                .OrderByDescending(user => user.Id)
                .Page(SummarizeUser(), 
                    PageSize, page);
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

        private Expression<Func<User,UserSummary>> SummarizeUser()
        {
            return user => new UserSummary
            {
                Id = user.Id,
                LoginName = user.UserName,
                CreatedAt = user.CreatedAtUtc,
                DisplayName = user.DisplayName
            };
        }
    }
}