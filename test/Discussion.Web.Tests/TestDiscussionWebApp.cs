using System;
using System.Collections.Generic;
using Discussion.Admin.Supporting;
using Discussion.Tests.Common;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Discussion.Web.Tests
{
    // Use shared context to maintain database fixture
    // see https://xunit.github.io/docs/shared-context.html#collection-fixture
    [CollectionDefinition("WebSpecs")]
    public class WebSpecsCollection : ICollectionFixture<TestDiscussionWebApp>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
    
    public class TestDiscussionWebApp : TestApplication
    {
        internal TestDiscussionWebApp(bool initialize)
        {
            if (initialize)
            {
                this.Init<Startup>();
            }
        }
        
        public TestDiscussionWebApp() : this(true)
        {
        }

        public new TestDiscussionWebApp Reset()
        {
            base.Reset();
            return this;
        }
    }
}