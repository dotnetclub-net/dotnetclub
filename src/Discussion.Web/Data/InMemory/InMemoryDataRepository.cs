using Jusfr.Persistent;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Discussion.Web.Data.InMemory
{
    public class InMemoryDataRepository<TEntry, TKey> : Repository<TEntry, TKey> where TEntry : class, IAggregate<TKey>
    {
        public InMemoryDataRepository(IRepositoryContext dataContext) : base(dataContext)
        {

        }

        private InMemoryResponsitoryContext StorageContext
        {
            get
            {
                return Context as InMemoryResponsitoryContext;
            }
        }

        public override IQueryable<TEntry> All
        {
            get
            {
                var repo = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
                return repo.Values.AsQueryable();
            }
        }

        public override bool Any(params Expression<Func<TEntry, bool>>[] predicates)
        {
            IQueryable<TEntry> query = All;
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }
            return query.Select(r => r.Id).Any();
        }

        public override void Create(TEntry entry)
        {
            var repo = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            entry.Id = GenerateNewId(repo.Count);
            repo.TryAdd(entry.Id, entry);
        }

        public override void Delete(IEnumerable<TEntry> entries)
        {
            var repo = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            var idList = entries.Where(e => e != null && repo.ContainsKey(e.Id)).Select(e => e.Id).ToArray();

            foreach (var id in idList)
            {
                TEntry val;
                repo.TryRemove(id, out val);
            }
        }

        public override void Delete(TEntry entry)
        {
            Delete(new[] { entry });
        }

        public override TReutrn Fetch<TReutrn>(Func<IQueryable<TEntry>, TReutrn> query)
        {
            return query(All);
        }

        public override IEnumerable<TEntry> Retrive(params TKey[] keys)
        {
            if (keys == null)
            {
                yield break;
            }

            var storage = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            foreach (var key in keys)
            {
                TEntry val;
                storage.TryGetValue(key, out val);
                yield return val;
            }
        }

        public override TEntry Retrive(TKey id)
        {
            return Retrive(new[] { id }).FirstOrDefault();
        }

        public override IEnumerable<TEntry> Retrive<TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys)
        {
            var storage = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            //var memberSelector = selector.Compile();
            //return All
            //    .Select(entry => new
            //    {
            //        Entry = entry,
            //        Member = memberSelector(entry)
            //    })
            //    .Where(item => keys.Contains(item.Member))
            //    .Select(item => item.Entry)
            //    .ToList();

            var parameters = selector.Parameters;
            var memberValue = Expression.Invoke(selector, parameters);

            Expression<Func<TMember, bool>> contains = member => keys.Contains(member);
            var containsMember = Expression.Invoke(contains, memberValue);
            var valueSelector = Expression.Lambda(containsMember, parameters) as Expression<Func<TEntry, bool>>;

            return All.Where(valueSelector).ToList();
        }

        public override IEnumerable<TEntry> Retrive<TMember>(string field, params TMember[] keys)
        {
            var entryParameter = Expression.Parameter(typeof(TEntry), "entry");
            var memberExpr = Expression.PropertyOrField(entryParameter, field);
            var selector = Expression.Lambda(memberExpr, entryParameter) as Expression<Func<TEntry, TMember>>;

            return Retrive(selector, keys);
        }

        public override void Save(IEnumerable<TEntry> entries)
        {
            var storage = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            var count = storage.Count;
            foreach (var item in entries)
            {
                if (item.Id.Equals(default(TKey)))
                {
                    item.Id = GenerateNewId(++count);
                }

                storage.AddOrUpdate(item.Id, item, (key, existingValue) => item);
            }
        }

        public override void Save(TEntry entry)
        {
            Save(new[] { entry });
        }

        public override void Update(IEnumerable<TEntry> entries)
        {
            var storage = StorageContext.GetRepositoryForEntity<TKey, TEntry>();
            foreach (var item in entries)
            {
                var existing = Retrive(item.Id);
                storage.TryUpdate(item.Id, item, existing);
            }
        }

        public override void Update(TEntry entry)
        {
            Update(new[] { entry });
        }

        TKey GenerateNewId(int count)
        {
            return (TKey)Convert.ChangeType(count + 1, typeof(TKey));
        }
    }

}
