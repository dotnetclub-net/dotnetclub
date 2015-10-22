using Xunit;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Discussion.Web.Tests
{
    public static class CollectionAssertions
    {
        public static void ShouldBeEmpty(this IEnumerable enumerable)
        {
            Assert.Empty(enumerable);
        }

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, T expected)
        {
            Assert.Contains<T>(expected, enumerable);
        }

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, T expected, Func<T, T, bool> comparer)
        {
            Assert.Contains<T>(expected, enumerable, new ObjectAssertions.ObjectComparer<T>(comparer));
        }

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, Func<T, bool> filter)
        {
            Assert.Contains<T>(enumerable, delegate (T obj)
            {
                return filter(obj);
            });
        }


        public static void ShouldNotEmpty(this IEnumerable enumerable)
        {
            Assert.NotEmpty(enumerable);
        }


        public static void ShouldNotContain<T>(this IEnumerable<T> enumerable, T expected)
        {
            Assert.DoesNotContain<T>(expected, enumerable);
        }

        public static void ShouldNotContain<T>(this IEnumerable<T> enumerable, T expected, Func<T, T, bool> comparer)
        {
            Assert.DoesNotContain<T>(expected, enumerable, new ObjectAssertions.ObjectComparer<T>(comparer));
        }

        public static void ShouldNotContain<T>(this IEnumerable<T> enumerable, Func<T, bool> filter)
        {
            Assert.DoesNotContain<T>(enumerable, delegate (T obj)
            {
                return filter(obj);
            });
        }
    }
}
