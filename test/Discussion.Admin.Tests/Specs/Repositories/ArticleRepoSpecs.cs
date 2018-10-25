using System;
using System.Linq;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Tests.Common.AssertionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Discussion.Admin.Tests.Specs.Repositories
{
    [Collection("AdminSpecs")]
    public class ArticleRepoSpecs
    {
        private readonly IServiceProvider services;
        public ArticleRepoSpecs(TestDiscussionAdminApp app)
        {
            services = app.ApplicationServices;
        }


        [Fact]
        public void should_store_an_article()
        {
            var article = new Article {Title = Guid.NewGuid().ToString() };
            var repo = services.GetRequiredService<IRepository<Article>>();

            repo.Save(article);

            article.Id.ShouldGreaterThan(0);            

            var articleGot = repo.Get(article.Id);
            articleGot.ShouldNotBeNull();
            articleGot.Title.ShouldEqual(article.Title);
        }
        
        
        [Fact]
        public void should_retrieve_articles()
        {
            var article = new Article {Title = Guid.NewGuid().ToString() };
            var repo = services.GetRequiredService<IRepository<Article>>();
            repo.Save(article);
            
            var articleList = repo.All().ToList();
           
            articleList.ShouldNotBeNull();
            Assert.Equal(1, articleList.Count);
            Assert.Equal(article.Title, articleList[0].Title);
        }

    }
    
}
