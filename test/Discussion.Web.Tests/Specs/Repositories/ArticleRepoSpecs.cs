using System;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Discussion.Web.Tests.Specs.Repositories
{

    [Collection("AppSpecs")]
    public class ArticleRepoSpecs
    {
        private readonly IServiceProvider _applicationServices;
        public ArticleRepoSpecs(TestApplication app)
        {
            _applicationServices = app.ApplicationServices;
        }


        [Fact]
        public void should_store_an_article()
        {
            var article = new Article() {Title = Guid.NewGuid().ToString() };
            var repo = _applicationServices.GetRequiredService<IRepository<Article>>();

            repo.Save(article);

            article.Id.ShouldGreaterThan(0);            

            var articleGot = repo.Get(article.Id);
            articleGot.ShouldNotBeNull();
            articleGot.Title.ShouldEqual(article.Title);
        }

    }
    
}
