using Discussion.Tests.Common;
using Xunit;

namespace Discussion.Web.Tests
{
    // Use shared context to maintain database fixture
    // see https://xunit.github.io/docs/shared-context.html#collection-fixture
    [CollectionDefinition("AppSpecs")]
    public class ApplicationCollection : ICollectionFixture<TestApplication>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
    
    public class TestApplication : TestDiscussionApplication
    {
        public TestApplication()
        {
            this.Init<Startup>();
        }

        public TestApplication Reset()
        {
            base.Reset();
            return this;
        }
    }
}