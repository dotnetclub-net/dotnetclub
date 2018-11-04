using System;
using Discussion.Core.Pagination;
using Discussion.Tests.Common.AssertionExtensions;
using Xunit;

namespace Discussion.Admin.Tests.CoreSpecs.Pagination
{
    public class PagingSpecs
    {
        [Fact]
        public void should_page_normally()
        {
            var paging = new Paging(10, 2, 1);
            
            paging.ItemCount.ShouldEqual(10);
            paging.PageSize.ShouldEqual(2);
            paging.CurrentPage.ShouldEqual(1);
        }
        
        
        [InlineData(2, 5)]
        [InlineData(1, 10)]
        [InlineData(11, 1)]
        [InlineData(9, 2)]
        [InlineData(50, 1)]
        [InlineData(3, 4)]
        [InlineData(4, 3)]
        [Theory]
        public void should_calculate_total_pages(int pageSize, int totalPages)
        {
            var paging = new Paging(10, pageSize);
            
            paging.TotalPages.ShouldEqual(totalPages);
        }
        
        [InlineData(1, false)]
        [InlineData(2, true)]
        [InlineData(-1, false)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(11, true)]
        [Theory]
        public void should_calculate_has_prev_page(int pageNumber, bool hasPrev)
        {
            var paging = new Paging(10, 2, pageNumber);
            
            paging.HasPreviousPage.ShouldEqual(hasPrev);
        }
        
        
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(-1, true)]
        [InlineData(4, true)]
        [InlineData(5, false)]
        [InlineData(6, false)]
        [InlineData(11, false)]
        [Theory]
        public void should_calculate_has_next_page(int pageNumber, bool hasNext)
        {
            var paging = new Paging(10, 2, pageNumber);
            
            paging.HasNextPage.ShouldEqual(hasNext);
        }
        
        [Fact]
        public void should_use_1_as_first_page()
        {
            Paging.FirstPage.ShouldEqual(1);
        }
        
        [InlineData(null, 1)]
        [InlineData(-1, 1)]
        [InlineData(0, 1)]
        [InlineData(-999, 1)]
        [InlineData(1, 1)]
        [InlineData(3, 3)]
        [InlineData(5, 5)]
        [InlineData(6, 5)]
        [InlineData(10000, 5)]
        [Theory]
        public void should_correct_page_number(int? pageNumber, int expectedPageNumber)
        {
            var paging = new Paging(10, 2, pageNumber);
            
            paging.CurrentPage.ShouldEqual(expectedPageNumber);
        }
        
        
        [InlineData(1, true)]
        [InlineData(3, true)]
        [InlineData(11, true)]
        [InlineData(-1, false)]
        [InlineData(0, false)]
        [InlineData(-222, false)]
        [Theory]
        public void should_validate_page_size(int pageSize, bool isValid)
        {
            Exception exception = null;
            try
            {
                new Paging(10, pageSize);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            
            
            if(isValid) exception.ShouldBeNull();
            if(!isValid) exception.ShouldNotBeNull();
        }
        
        [InlineData(1, true)]
        [InlineData(3, true)]
        [InlineData(0, true)]
        [InlineData(999, true)]
        [InlineData(-12, false)]
        [InlineData(-222, false)]
        [Theory]
        public void should_throw_on_invalid_item_count(int itemCount, bool isValid)
        {
            Exception exception = null;
            try
            {
                new Paging(itemCount, 3);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            
            
            if(isValid) exception.ShouldBeNull();
            if(!isValid) exception.ShouldNotBeNull();
        }
    }
}