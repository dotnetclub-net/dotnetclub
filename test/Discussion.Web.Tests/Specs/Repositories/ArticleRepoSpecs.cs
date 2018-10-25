using System;
using Discussion.Core.Data;
using Discussion.Core.Models;
using Discussion.Tests.Common;
using Discussion.Tests.Common.AssertionExtensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Discussion.Web.Tests.Specs.Repositories
{

    [Collection("WebSpecs")]
    public class ArticleRepoSpecs
    {
        private readonly IServiceProvider _applicationServices;
        public ArticleRepoSpecs(TestDiscussionWebApp app)
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
