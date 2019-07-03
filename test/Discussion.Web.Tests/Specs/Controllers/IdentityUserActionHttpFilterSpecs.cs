using System.Threading.Tasks;
using Discussion.Core.Utilities;
using Discussion.Core.ViewModels;
using Discussion.Tests.Common;
using Discussion.Web.Controllers;
using Discussion.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Discussion.Web.Tests.Specs.Controllers
{
    [Collection("WebSpecs")]
    public class IdentityUserActionHttpFilterSpecs
    {
        private readonly TestDiscussionWebApp _app;

        public IdentityUserActionHttpFilterSpecs(TestDiscussionWebApp app)
        {
            _app = app;
        }


        
    }
}