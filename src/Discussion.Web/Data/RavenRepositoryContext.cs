using Jusfr.Persistent;
using Raven.Client;
using System;

namespace Discussion.Web.Data
{
    public class RavenRepositoryContext : IRepositoryContext
    {
        Func<IDocumentStore> _store;
        IDocumentSession _session;
        bool _isRolledBack = false;

        public RavenRepositoryContext(Func<IDocumentStore> storeFactory)
        {
            _store = storeFactory;
        }
        
        public bool DistributedTransactionSupported
        {
            get
            {
                return false;
            }
        }

        public IDocumentSession DemandSession() {
            return _session ?? (_session = _store().OpenSession());
        }

        public Guid ID { get; } = Guid.NewGuid();

        public void Begin()
        {
            DemandSession();
        }

        public void Commit()
        {
            var session = this.DemandSession();
            session.SaveChanges();
        }

        public void Dispose()
        {
            if (_session != null)
            {
                if (!_isRolledBack)
                {
                    Commit();
                }
                

                _session.Dispose();
                _session = null;
            }
        }

        public void Rollback()
        {
            _isRolledBack = true;
        }
    }
}
