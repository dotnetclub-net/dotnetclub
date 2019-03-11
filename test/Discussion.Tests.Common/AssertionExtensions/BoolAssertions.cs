using Xunit;

namespace Discussion.Tests.Common.AssertionExtensions
{
    public static class BoolAssertions
    {
        public static void ShouldBeTrue(this bool actual)
        {
            Assert.True(actual);
        }

        public static void ShouldBeFalse(this bool actual)
        {
            Assert.False(actual);
        }
    }
}
