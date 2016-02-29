using System;
using Discussion.Web.Models;
using Jusfr.Persistent.Mongo;

namespace Discussion.Web.Repositories
{
    public class ArticleRepository
    {
        private MongoRepositoryContext context;

        public ArticleRepository()
        {
            var conStr = "mongodb://127.0.0.1:27017/test";
            context = new MongoRepositoryContext(conStr);
            context.Database.DropCollection<Article>();
        }

        public void Create(Article article)
        {
            var articleRepo = new MongoRepository<Article>(context);
            articleRepo.Create(article);
        }


        public Article Get(int id)
        {
            var articleRepo = new MongoRepository<Article>(context);
            return articleRepo.Retrive(id);
        }

    }
}