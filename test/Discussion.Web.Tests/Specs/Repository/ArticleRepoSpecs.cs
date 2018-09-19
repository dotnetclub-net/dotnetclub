using Discussion.Web.Models;
using Jusfr.Persistent;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Discussion.Web.Tests.Specs.Repository
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

            repo.Create(article);

            article.Id.ShouldGreaterThan(0);            

            var articleGot = repo.Retrive(article.Id);
            articleGot.ShouldNotBeNull();
            articleGot.Title.ShouldEqual(article.Title);
        }

    }
    
}
