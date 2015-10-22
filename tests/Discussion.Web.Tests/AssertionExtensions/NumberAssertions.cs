using Xunit;

namespace Discussion.Web.Tests
{
    public static class NumberAssertions
    {
        public static void ShouldEqual(this int number, int expected)
        {
            Assert.Equal(expected, number);
        }

        public static void ShouldNotEqual(this int number, int expected)
        {
            Assert.NotEqual(expected, number);
        }

        public static void ShouldGreaterThan(this int number, int other)
        {
            Assert.True(number > other, $"Expected {number} to be greater than {other}");
        }

        public static void ShouldLessThan(this int number, int other)
        {
            Assert.True(number < other, $"Expected {number} to be less than {other}");
        }

        public static void ShouldGreaterOrEquals(this int number, int other)
        {
            Assert.True(number >= other, $"Expected {number} to be greater than or equal {other}");
        }

        public static void ShouldLessOrEquals(this int number, int other)
        {
            Assert.True(number <= other, $"Expected {number} to be less than or equal {other}");
        }
        






        public static void ShouldEqual(this long number, long expected)
        {
            Assert.Equal(expected, number);
        }

        public static void ShouldNotEqual(this long number, long expected)
        {
            Assert.NotEqual(expected, number);
        }

        public static void ShouldGreaterThan(this long number, long other)
        {
            Assert.True(number > other, $"Expected {number} to be greater than {other}");
        }

        public static void ShouldLessThan(this long number, long other)
        {
            Assert.True(number < other, $"Expected {number} to be less than {other}");
        }

        public static void ShouldGreaterOrEquals(this long number, long other)
        {
            Assert.True(number >= other, $"Expected {number} to be greater than or equal {other}");
        }

        public static void ShouldLessOrEquals(this long number, long other)
        {
            Assert.True(number <= other, $"Expected {number} to be less than or equal {other}");
        }

    }
}
