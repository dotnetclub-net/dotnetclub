using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Discussion.Core.Pagination
{
    public class Paged<T>
    {
        internal Paged()
        {
            
        }
        
        public T[] Items { get; set; }
        
        public Paging Paging { get; set; }

        public int TotalItemCount => Paging.ItemCount;

    }
    
    public static class PageExtensions
    {
        public static Paged<T> Page<T>(this IEnumerable<T> enumerableItems, int pageSize, int? pageNumber = null)
        {
            return Page(enumerableItems, item => item, pageSize, pageNumber);
        }
  
        public static Paged<TResult> Page<TSource, TResult>(this IEnumerable<TSource> enumerableItems, Func<TSource, TResult> valueSelector, int pageSize, int? pageNumber = null)
        {
            var (paging, pagedQuery) = PageFromQuery(enumerableItems.AsQueryable(), pageSize, pageNumber);
            var items = pagedQuery.AsEnumerable().Select(valueSelector).ToArray();

            return new Paged<TResult>
            {
                Paging = paging,
                Items = items
            };
        }
        
        public static Paged<T> Page<T>(this IQueryable<T> queryableItems, int pageSize, int? pageNumber = null)
        {
            Expression<Func<T, T>> selector = item => item;
            return Page(queryableItems, selector, pageSize, pageNumber);
        }
        
        public static Paged<TResult> Page<TSource, TResult>(this IQueryable<TSource> queryableItems, Expression<Func<TSource, TResult>> valueSelector, int pageSize, int? pageNumber = null)
        {
            var (paging, pagedQuery) = PageFromQuery(queryableItems, pageSize, pageNumber);
            var items = pagedQuery.Select(valueSelector).ToArray();

            return new Paged<TResult>
            {
                Paging = paging,
                Items = items
            };
        }
        
        public static Paged<TResult> Page<TSource, TResult>(this IQueryable<TSource> queryableItems, Func<TSource, TResult> valueSelector, int pageSize, int? pageNumber = null)
        {
            var (paging, pagedQuery) = PageFromQuery(queryableItems, pageSize, pageNumber);
            var items = pagedQuery.ToArray().Select(valueSelector).ToArray();

            return new Paged<TResult>
            {
                Paging = paging,
                Items = items
            };
        }

        private static (Paging paging, IQueryable<TSource> pagedQuery) PageFromQuery<TSource>(IQueryable<TSource> queryableItems, int pageSize, int? pageNumber)
        {
            var count = queryableItems.Count();
            var paging = new Paging(count, pageSize, pageNumber);

            var skip = (paging.CurrentPage - 1) * pageSize;
            var pagedQuery = queryableItems.Skip(skip).Take(paging.PageSize);
            return (paging, pagedQuery);
        }
    }
}