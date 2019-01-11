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
        const int TopicPageSize = 20;
        private readonly IRepository<User> _userRepo;

        public DiscussionUserManagementController(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        [Route("api/discussion-users")]
        public Paged<UserSummary> List(int? page = 1)
        {
            throw new System.NotImplementedException();
        }
        
        [Route("api/discussion-users/{id}")]
        [HttpDelete]
        public ApiResponse Block(int id, DateTime? autoUnblockAt)
        {
            throw new System.NotImplementedException();
        }
        
        [Route("api/discussion-users/{id}")]
        [HttpDelete]
        public ApiResponse UnBlock(int id)
        {
            throw new System.NotImplementedException();
        }

    }
}