using System;
using System.Collections.Generic;
using Xunit;
using static Xunit.Assert;

namespace Discussion.Web.Tests
{
    public static class ObjectAssertions
    {

        #region Equality

        public static void ShouldEqual(this string obj, string expected)
        {
            Equal(expected, obj);
        }

        public static void ShouldEqual(this string actual, string expected, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false)
        {
            Equal(expected, actual, ignoreCase, ignoreLineEndingDifferences, ignoreWhiteSpaceDifferences);
        }

        public static void ShouldEqual<T>(this T obj, T expected)
        {
            Equal(expected, obj);
        }

        public static void ShouldEqual<T>(this T obj, T expected, Func<T, T, bool> comparer)
        {
            Equal(expected, obj, new ObjectComparer<T>(comparer));
        }

        public static void ShouldEqual(this object obj, object expected)
        {
            Equal(expected, obj);
        }
        

        public static void ShouldNotEqual(this string obj, string expected)
        {
            NotEqual(expected, obj);
        }

        public static void ShouldNotEqual<T>(this T obj, T expected)
        {
            NotEqual(expected, obj);
        }

        public static void ShouldNotEqual<T>(this T obj, T expected, Func<T, T, bool> comparer)
        {
            NotEqual(expected, obj, new ObjectComparer<T>(comparer));
        }

        public static void ShouldNotEqual(this object obj, object expected)
        {
            NotEqual(expected, obj);
        }

        #endregion


        public static void ShouldBeNull(this object obj)
        {
            Null(obj);
        }

        public static void ShouldNotBeNull(this object obj)
        {
            NotNull(obj);
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
