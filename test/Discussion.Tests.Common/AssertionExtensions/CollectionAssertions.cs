using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Discussion.Tests.Common.AssertionExtensions
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

        public static void ShouldContain(this string obj, string expectedSubstring)
        {
            Assert.Contains(expectedSubstring, obj);
        }

        public static void ShouldContain(this string obj, string expectedSubstring, StringComparison comparisonType)
        {
            Assert.Contains(expectedSubstring, obj, comparisonType);
        }

        public static void ShouldNotContain(this string obj, string expectedSubstring)
        {
            Assert.DoesNotContain(expectedSubstring, obj);
        }

        public static void ShouldNotContain(this string obj, string expectedSubstring, StringComparison comparisonType)
        {
            Assert.DoesNotContain(expectedSubstring, obj, comparisonType);
        }
    }
}
