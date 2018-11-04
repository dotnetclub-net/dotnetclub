using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Discussion.Core.Pagination
{
    public class Paged<T>
    {
        private Paged()
        {
            
        }
        
        public T[] Items { get; set; }
        
        public Paging Paging { get; set; }

        public int Count => Paging.ItemCount;

 
        public static Paged<T> ForList(IEnumerable<T> queryableItems, int pageSize, int? pageNumber = null)
        {
            return ForList(queryableItems, item => item, pageSize, pageNumber);
        }
  
        public static Paged<T> ForList<TSource>(IEnumerable<TSource> queryableItems, Func<TSource, T> transformer, int pageSize, int? pageNumber = null)
        {
            var (paging, pagedQuery) = PageFromQuery(queryableItems.AsQueryable(), pageSize, pageNumber);
            var items = pagedQuery.AsEnumerable().Select(transformer).ToArray();

            return new Paged<T>
            {
                Paging = paging,
                Items = items
            };
        }             
        
        public static Paged<T> ForQuery(IQueryable<T> queryableItems, int pageSize, int? pageNumber = null)
        {
            return ForQuery(queryableItems, item => item, pageSize, pageNumber);
        }
        
        public static Paged<T> ForQuery<TSource>(IQueryable<TSource> queryableItems, Expression<Func<TSource, T>> transformer, int pageSize, int? pageNumber = null)
        {
            var (paging, pagedQuery) = PageFromQuery(queryableItems, pageSize, pageNumber);
            var items = pagedQuery.Select(transformer).ToArray();

            return new Paged<T>
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