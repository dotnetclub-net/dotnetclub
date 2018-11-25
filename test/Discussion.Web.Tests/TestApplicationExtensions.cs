using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Mvc;
using Discussion.Tests.Common;

namespace Discussion.Web.Tests
{
    public static class TestApplicationExtensions
    {
        public static User GetDiscussionUser(this TestApplication app)
        {
            var userRepo = app.GetService<IRepository<User>>();
            return app.User.ToDiscussionUser(userRepo);
        }
    }
}