using System;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Discussion.Admin.Tests
{
    // Use shared context to maintain database fixture
    // see https://xunit.github.io/docs/shared-context.html#collection-fixture
    [CollectionDefinition("AdminSpecs")]
    public class AdminSpecsCollection : ICollectionFixture<TestDiscussionAdminApp>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
    
    public class TestDiscussionAdminApp : TestApplication
    {
        internal TestDiscussionAdminApp(bool initialize)
        {
            if (initialize)
            {
                InitAdminApp();
            }
        }

        private void InitAdminApp()
        {
            var connectionStringEVKey = $"DOTNETCLUB_{ApplicationDataServices.ConfigKeyConnectionString}";
            Environment.SetEnvironmentVariable(connectionStringEVKey, "Data Source=:memory:");

            this.Init<Startup>();

            var dbContext = ApplicationServices.GetService<ApplicationDbContext>();
            dbContext.Database.OpenConnection();
            dbContext.Database.EnsureCreated();
        }

        public TestDiscussionAdminApp() : this(true)
        {
        }

        public new TestDiscussionAdminApp Reset()
        {
            base.Reset();
            return this;
        }
    }
}