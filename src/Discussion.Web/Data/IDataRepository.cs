using Jusfr.Persistent;

namespace Discussion.Web.Data
{
    public interface IDataRepository<TEntity> : IRepository<TEntity, TEntity> where TEntity: IEntry
    {
    }
}
