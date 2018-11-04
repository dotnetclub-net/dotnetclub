using Discussion.Core.Utilities;

namespace Discussion.Core.Pagination
{
    public class Paging
    {
        public const int FirstPage = 1;
        
        public Paging(int itemCount, int pageSize, int? currentPage = null)
        {
            ArgGuard.MakeSure(itemCount >= 0, nameof(itemCount));
            ArgGuard.MakeSure(pageSize >= 1, nameof(pageSize));
            
            var (totalPages, actualCurrentPage) = NormalizePaging(itemCount, pageSize, currentPage);

            this.ItemCount = itemCount;
            this.PageSize = pageSize;
            this.TotalPages = totalPages;
            this.CurrentPage = actualCurrentPage;
        }
        
        public int PageSize { get;}
        public int ItemCount { get; }
        
        public int CurrentPage { get;}
        public int TotalPages { get; }
        
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > FirstPage;
        
        
        private static (int, int) NormalizePaging(int itemCount, int pageSize, int? currentPage = null)
        {
            var isValidCurrentPage = currentPage != null && currentPage.Value >= FirstPage;
            var actualPage = isValidCurrentPage ? currentPage.Value : FirstPage;

            var basePage = itemCount / pageSize;
            var allPage = itemCount % pageSize == 0 ? basePage : basePage + 1;
            if (actualPage > allPage)
            {
                actualPage = allPage;
            }

            return (allPage, actualPage);
        }
    }
}