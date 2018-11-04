using System;
using System.Collections.Generic;
using Xunit;

namespace Discussion.Tests.Common.AssertionExtensions
{
    public static class ObjectAssertions
    {

        #region Equality

        public static void ShouldEqual(this string obj, string expected)
        {
            Assert.Equal(expected, obj);
        }

        public static void ShouldEqual(this string actual, string expected, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
        {
            Assert.Equal(expected, actual, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
        }

        public static void ShouldEqual<T>(this T obj, T expected)
        {
            Assert.Equal(expected, obj);
        }

        public static void ShouldEqual<T>(this T obj, T expected, Func<T, T, bool> comparer)
        {
            Assert.Equal(expected, obj, new ObjectComparer<T>(comparer));
        }

        public static void ShouldEqual(this object obj, object expected)
        {
            Assert.Equal(expected, obj);
        }
        

        public static void ShouldNotEqual(this string obj, string expected)
        {
            Assert.NotEqual(expected, obj);
        }

        public static void ShouldNotEqual<T>(this T obj, T expected)
        {
            Assert.NotEqual(expected, obj);
        }

        public static void ShouldNotEqual<T>(this T obj, T expected, Func<T, T, bool> comparer)
        {
            Assert.NotEqual(expected, obj, new ObjectComparer<T>(comparer));
        }

        public static void ShouldNotEqual(this object obj, object expected)
        {
            Assert.NotEqual(expected, obj);
        }

        #endregion


        public static void ShouldBeNull(this object obj)
        {
            Assert.Null(obj);
        }

        public static void ShouldNotBeNull(this object obj)
        {
            Assert.NotNull(obj);
        }

        public static void IsType(this object obj, Type expectedType)
        {
            Assert.IsType(expectedType, obj);
        }

        public static void IsType<T>(this object obj)
        {
            Assert.IsType<T>(obj);
        }

        public static void IsNotType(this object obj, Type expectedType)
        {
            Assert.IsNotType(expectedType, obj);
        }

        public static void IsNotType<T>(this object obj)
        {
            Assert.IsNotType<T>(obj);
        }

        public class ObjectComparer<T> : EqualityComparer<T>
        {
            private Func<T, T, bool> _comparer;
            public ObjectComparer(Func<T, T, bool> comparer)
            {
                if (comparer == null)
                {
                    throw new NullReferenceException(nameof(comparer));
                }

                _comparer = comparer;
            }


            public override bool Equals(T x, T y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null) && ReferenceEquals(y, null)) return true;
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;

                return _comparer(x, y);
            }

            public override int GetHashCode(T obj)
            {
                if (ReferenceEquals(obj, null)) return 0;

                return obj.GetHashCode();
            }
        }
    }
}
