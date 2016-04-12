using Discussion.Web.Models;
using Discussion.Web.Repositories;
using Jusfr.Persistent;
using Jusfr.Persistent.Mongo;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Discussion.Web.Tests.Specs.Repository
{

    [Collection("DbSpecs")]
    public class ArticleRepoSpecs
    {
        private readonly IServiceProvider _applicationServices;
        public ArticleRepoSpecs(Database database)
        {
            _applicationServices = StartupSpecs.ServicesSpecs.CreateApplicationServices(services =>
            {
                services.AddScoped(typeof(IRepositoryContext), (serviceProvider) => database.Context);
            });
        }


        [Fact]
        public void should_store_an_article()
        {
            var article = new Article() {Title = Guid.NewGuid().ToString() };
            var repo = _applicationServices.GetRequiredService<IDataRepository<Article>>();

            repo.Create(article);

            article.Id.ShouldGreaterThan(0);            

            var articleGot = repo.Retrive(article.Id);
            articleGot.ShouldNotBeNull();
            articleGot.Title.ShouldEqual(article.Title);
        }

    }
    
}
