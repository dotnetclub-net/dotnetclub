using Discussion.Web.Models;
using Jusfr.Persistent.Mongo;
using Jusfr.Persistent;

namespace Discussion.Web.Repositories
{
    public class ArticleRepository
    {
        private IRepositoryContext _context;

        public ArticleRepository(IRepositoryContext repositoryContext)
        {
            _context = repositoryContext;
        }


        public void Create(Article article)
        {
            var articleRepo = new MongoRepository<Article>(_context);
            articleRepo.Create(article);
        }


        public Article Get(int id)
        {
            var articleRepo = new MongoRepository<Article>(_context);
            return articleRepo.Retrive(id);
        }

    }
}