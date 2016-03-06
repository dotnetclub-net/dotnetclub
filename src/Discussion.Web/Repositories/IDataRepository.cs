using Jusfr.Persistent;

namespace Discussion.Web.Repositories
{
    public interface IDataRepository<TEntity> : IRepository<TEntity, TEntity> where TEntity: IEntry
    {
    }
}
