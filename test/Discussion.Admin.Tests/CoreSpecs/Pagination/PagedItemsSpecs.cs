using System;
using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Core.Pagination;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Xunit;

namespace Discussion.Admin.Tests.CoreSpecs.Pagination
{
    [Collection("AdminSpecs")]
    public class PagedItemsSpecs
    {
        const int PageSize = 3;
        private readonly TestDiscussionAdminApp _adminApp;

        public PagedItemsSpecs(TestDiscussionAdminApp adminApp)
        {
            _adminApp = adminApp;
        }

        [Fact]
        public void should_create_for_list()
        {
            var items = Enumerable.Range(1, 30)
                                .Select(number => $"item {number}")
                                .ToArray();

            var pagedList = Paged<string>.ForList(items, PageSize, 2);
            
            VerifyPaging(pagedList, 2);
            pagedList.Items
                .SequenceEqual(new[] { "item 4", "item 5", "item 6" })
                .ShouldEqual(true);
        }

        [Fact]
        public void should_create_for_list_with_mapper()
        {
            var items = Enumerable.Range(1, 30)
                                .Select(number => $"item {number}")
                                .ToArray();

            var pagedList = Paged<int>.ForList(items, 
                                        item => int.Parse(item.Substring("item".Length + 1)),
                                        PageSize, 10);
            
            VerifyPaging(pagedList, 10);
            pagedList.Items
                .SequenceEqual(new[] { 28, 29, 30 })
                .ShouldEqual(true);
        }



        [Fact]
        public void should_create_for_queryable()
        {
            Create30Articles();
            
            
            var sortedArticles = _adminApp.GetService<IRepository<Article>>().All().OrderBy(a => a.Id);
            var pagedList = Paged<Article>.ForList(sortedArticles, PageSize, 2);
            
            VerifyPaging(pagedList, 2);
            pagedList.Items
                .Select(article => article.Title)
                .SequenceEqual(new[] { "queryable 4", "queryable 5", "queryable 6" })
                .ShouldEqual(true);
        }
        
        
        [Fact]
        public void should_create_for_queryable_with_mapper()
        {
            Create30Articles();
            
            var sortedArticles = _adminApp.GetService<IRepository<Article>>().All().OrderBy(a => a.Id);
            var pagedList = Paged<int>.ForQuery(sortedArticles, 
                article => int.Parse(article.Title.Substring("queryable".Length + 1)),
                PageSize, 1);


            VerifyPaging(pagedList, 1);
            pagedList.Items
                .SequenceEqual(new[] { 1, 2, 3 })
                .ShouldEqual(true);
        }


        private void Create30Articles()
        {
            var repo = _adminApp.GetService<IRepository<Article>>();
            
            _adminApp.DeleteAll<Article>();
            Enumerable.Range(1, 30)
                .Select(number => $"queryable {number}")
                .Select(title => new Article {Title = title})
                .ToList()
                .ForEach(article => repo.Save(article));
        }

        private static void VerifyPaging<T>(Paged<T> pagedList, int currentPage)
        {
            pagedList.Count.ShouldEqual(30);
            pagedList.Items.Length.ShouldEqual(PageSize);
            pagedList.Paging.CurrentPage.ShouldEqual(currentPage);
        }
    }
}