using Jusfr.Persistent;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discussion.Web.Repositories
{
    public class BaseDataRepository<TEntity> : IDataRepository<TEntity> where TEntity : class, IEntry, IAggregate<int>
    {
        IRepository<TEntity, TEntity, int> _repoImplemention;
        public BaseDataRepository(IRepository<TEntity, TEntity, int> repoImplemention)
        {
            _repoImplemention = repoImplemention;
        }

        public virtual IQueryable<TEntity> All
        {
            get
            {
                return _repoImplemention.All;
            }
        }

        public virtual void Create(TEntity entry)
        {
            _repoImplemention.Create(entry);
        }

        public virtual void Delete(IEnumerable<TEntity> entries)
        {
            _repoImplemention.Delete(entries);
        }

        public virtual void Delete(TEntity entry)
        {
            _repoImplemention.Delete(entry);
        }

        public virtual TReutrn Fetch<TReutrn>(Func<IQueryable<TEntity>, TReutrn> query)
        {
            return _repoImplemention.Fetch(query);
        }

        public virtual IEnumerable<TEntity> Retrive(params int[] keys)
        {
            return _repoImplemention.Retrive(keys);
        }

        public virtual TEntity Retrive(int id)
        {
            return _repoImplemention.Retrive(id);
        }

        public virtual IEnumerable<TEntity> Retrive<TKey>(string field, params TKey[] keys)
        {
            return _repoImplemention.Retrive(field, keys);
        }

        public virtual void Save(IEnumerable<TEntity> entries)
        {
            _repoImplemention.Save(entries);
        }

        public virtual void Save(TEntity entry)
        {
            _repoImplemention.Save(entry);
        }

        public virtual void Update(IEnumerable<TEntity> entries)
        {
            _repoImplemention.Update(entries);
        }

        public virtual void Update(TEntity entry)
        {
            _repoImplemention.Update(entry);
        }
    }
}
