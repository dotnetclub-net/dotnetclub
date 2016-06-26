using Jusfr.Persistent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Discussion.Web.Data.InMemory
{
    public class InMemoryDataRepository<TEntry> : Repository<TEntry> where TEntry : Entity
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
                var repo = StorageContext.GetRepositoryForEntity<TEntry>();
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
            var repo = StorageContext.GetRepositoryForEntity<TEntry>();
            entry.Id = GenerateNewId(repo.Count);
            repo.TryAdd(entry.Id, entry);
        }

        public override void Delete(TEntry entry)
        {
            var repo = StorageContext.GetRepositoryForEntity<TEntry>();

            TEntry val;
            repo.TryRemove(entry.Id, out val);
        }

        public override TReutrn Fetch<TReutrn>(Func<IQueryable<TEntry>, TReutrn> query)
        {
            return query(All);
        }

        public override IEnumerable<TEntry> Retrive(params object[] keys)
        {
            if (keys == null)
            {
                yield break;
            }

            var storage = StorageContext.GetRepositoryForEntity<TEntry>();
            foreach (var key in keys)
            {
                TEntry val;
                storage.TryGetValue((int)key, out val);
                yield return val;
            }
        }

        public override TEntry Retrive(object id)
        {
            return Retrive(new[] { id }).FirstOrDefault();
        }

        public override IEnumerable<TEntry> Retrive<TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys)
        {
            var storage = StorageContext.GetRepositoryForEntity<TEntry>();
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

            var criteria = FilterByMemberCriteriaComposer.Compose(selector, keys);
            return All.Where(criteria).ToList();
        }

        public override IEnumerable<TEntry> Retrive<TMember>(string field, params TMember[] keys)
        {
            var criteria = FilterByMemberCriteriaComposer.ComposeByField<TEntry, TMember>(field, keys);
            return All.Where(criteria).ToArray();
        }

        public override void Save(TEntry entry)
        {
            var storage = StorageContext.GetRepositoryForEntity<TEntry>();
            var count = storage.Count;
            if (entry.Id.Equals(default(int)))
            {
                entry.Id = GenerateNewId(++count);
            }

            storage.AddOrUpdate(entry.Id, entry, (key, existingValue) => entry);
        }

        public override void Update(TEntry entry)
        {
            var storage = StorageContext.GetRepositoryForEntity<TEntry>();
            var existing = Retrive(entry.Id);
            storage.TryUpdate(entry.Id, entry, existing);
        }

        int GenerateNewId(int count)
        {
            return (int)Convert.ChangeType(count + 1, typeof(int));
        }
    }

}
