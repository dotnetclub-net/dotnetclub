using Jusfr.Persistent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Raven.Client.Documents.Session;

namespace Discussion.Web.Data
{
    public class RavenDataRepository<TEntry> : Repository<TEntry> where TEntry : Entity
    {
        private readonly RavenRepositoryContext _ravenContext;
        private IDocumentSession _session;


        public RavenDataRepository(IRepositoryContext context) : base(context)
        {
            _ravenContext = context as RavenRepositoryContext;
            if(_ravenContext == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
        }

        public IDocumentSession Session
        {
            get
            {
                return _session ?? (_session = _ravenContext.DemandSession());
            }
        }

        public override IQueryable<TEntry> All
        {
            get
            {
                return Session.Query<TEntry>();
            }
        }

        public override bool Any(params Expression<Func<TEntry, bool>>[] predicates)
        {
            IQueryable<TEntry> query = All;
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }

            return query.Any();
        }

        public override void Create(TEntry entry)
        {
            if(entry.Id > 0)
            {
                throw new InvalidOperationException("Could not create an existing entity again.");
            }

            Session.Store(entry);
        }

        public override void Delete(TEntry entry)
        {
            if (entry.Id <= 0)
            {
                throw new InvalidOperationException("Could not delete an entity with a negative identity.");
            }

            Session.Delete<TEntry>(entry);
        }

        public override TReutrn Fetch<TReutrn>(Func<IQueryable<TEntry>, TReutrn> query)
        {
            return query(All);
        }

        public override IEnumerable<TEntry> Retrive(object[] keys)
        {
            return Session.Load<TEntry>(keys.Select(k => k.ToString())).Values;
        }

        public override TEntry Retrive(object id)
        {
            // conventions: int id  ->  string id
            return Session.Load<TEntry>(id.ToString());
        }

        public override IEnumerable<TEntry> Retrive<TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys)
        {
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
            if (entry.Id > 0)
            {
                Update(entry);
            }
            else
            {
                Create(entry);
            }
        }

        public override void Update(TEntry entry)
        {
            if (entry.Id <= 0)
            {
                throw new InvalidOperationException("Could not create an existing entity again.");
            }

            Session.Store(entry);
        }
    }
}
