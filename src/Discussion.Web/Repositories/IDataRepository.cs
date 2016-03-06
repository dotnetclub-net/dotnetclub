using Jusfr.Persistent;

namespace Discussion.Web.Repositories
{
    interface IDataRepository<TEntity> : IRepository<TEntity, TEntity> where TEntity: IEntry
    {
    }
}
