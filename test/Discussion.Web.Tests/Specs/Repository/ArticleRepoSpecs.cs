using Discussion.Web.Models;
using Discussion.Web.Repositories;
using System;
using Xunit;

namespace Discussion.Web.Tests.Specs.Repository
{
    public class ArticleRepoSpecs
    {

        [Fact]
        public void should_store_an_article()
        {
            var article = new Article() {Title = Guid.NewGuid().ToString() };
            var repo = new ArticleRepository();

            repo.Create(article);

            article.Id.ShouldGreaterThan(0);



            var articleGot = repo.Get(article.Id);
            articleGot.ShouldNotBeNull();
            articleGot.Title.ShouldEqual(article.Title);
        }

    }
}
