using System;
using System.Collections.Generic;
using Discussion.Admin.Supporting;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        internal const string JwtIssuer = "testing";
        internal const string JwtAudience = "specs";
        
        public TestDiscussionAdminApp()
        {
            InitAdminApp();
        }
        
        private void InitAdminApp()
        {
            var connectionStringEVKey = $"DOTNETCLUB_{ApplicationDataServices.ConfigKeyConnectionString}";
            Environment.SetEnvironmentVariable(connectionStringEVKey, "Data Source=:memory:");

            this.Init<Startup>(hostBuilder =>
            {
                hostBuilder.ConfigureAppConfiguration((host, config) =>
                {
                    var jwtOptions = new Dictionary<string, string>()
                    {
                        {"JwtIssuerOptions:Secret", Guid.NewGuid().ToString()},
                        {$"JwtIssuerOptions:{nameof(JwtIssuerOptions.Issuer)}", JwtIssuer},
                        {$"JwtIssuerOptions:{nameof(JwtIssuerOptions.Audience)}", JwtAudience}
                    };

                    config.AddInMemoryCollection(jwtOptions);
                });
            });

            var dbContext = ApplicationServices.GetService<ApplicationDbContext>();
            dbContext.Database.OpenConnection();
            dbContext.Database.EnsureCreated();
        }


        public new TestDiscussionAdminApp Reset()
        {
            base.Reset();
            return this;
        }
    }
}