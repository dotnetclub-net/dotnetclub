using static Xunit.Assert;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Discussion.Web.Tests
{
    public static class CollectionAssertions
    {
        public static void ShouldBeEmpty(this IEnumerable enumerable)
        {
            Empty(enumerable);
        }

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, T expected)
        {
            Contains<T>(expected, enumerable);
        }

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, T expected, Func<T, T, bool> comparer)
        {
            Contains<T>(expected, enumerable, new ObjectAssertions.ObjectComparer<T>(comparer));
        }

        public static void ShouldContain<T>(this IEnumerable<T> enumerable, Func<T, bool> filter)
        {
            Contains<T>(enumerable, delegate (T obj)
            {
                return filter(obj);
            });
        }


        public static void ShouldNotEmpty(this IEnumerable enumerable)
        {
            NotEmpty(enumerable);
        }


        public static void ShouldNotContain<T>(this IEnumerable<T> enumerable, T expected)
        {
            DoesNotContain<T>(expected, enumerable);
        }

        public static void ShouldNotContain<T>(this IEnumerable<T> enumerable, T expected, Func<T, T, bool> comparer)
        {
            DoesNotContain<T>(expected, enumerable, new ObjectAssertions.ObjectComparer<T>(comparer));
        }

        public static void ShouldNotContain<T>(this IEnumerable<T> enumerable, Func<T, bool> filter)
        {
            DoesNotContain<T>(enumerable, delegate (T obj)
            {
                return filter(obj);
            });
        }
    }
}
