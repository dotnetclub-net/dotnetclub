using Discussion.Web.Models;
using Discussion.Web.Repositories;
using Jusfr.Persistent.Mongo;
using System;
using Xunit;

namespace Discussion.Web.Tests.Specs.Repository
{

    [Collection("DbSpec")]
    public class ArticleRepoSpecs
    {
        private Database _database;
        public ArticleRepoSpecs(Database database)
        {
            _database = database;
        }


        [Fact]
        public void should_store_an_article()
        {
            var article = new Article() {Title = Guid.NewGuid().ToString() };
            var mongoRepo = new MongoRepository<Article>(_database.Context);
            var repo = new BaseDataRepository<Article>(mongoRepo);

            repo.Create(article);

            article.Id.ShouldGreaterThan(0);            

            var articleGot = repo.Retrive(article.Id);
            articleGot.ShouldNotBeNull();
            articleGot.Title.ShouldEqual(article.Title);
        }
        

    }
    
}
