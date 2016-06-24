using Jusfr.Persistent;
using System;
using System.Collections.Concurrent;
using System.Collections;

namespace Discussion.Web.Data.InMemory
{

    public class InMemoryResponsitoryContext : IDisposable, IRepositoryContext
    {
        private ConcurrentDictionary<string, object> _storage = new ConcurrentDictionary<string, object>();

        public bool DistributedTransactionSupported { get; } = false;

        public ConcurrentDictionary<int, TEntry> GetRepositoryForEntity<TEntry>()
        {
            var type = typeof(TEntry).FullName;
            var entryStorage = _storage.GetOrAdd(type, typeName =>
            {
                return new ConcurrentDictionary<int, TEntry>();
            });

            return entryStorage as ConcurrentDictionary<int, TEntry>;
        }

        public Guid ID { get; } = Guid.NewGuid();

        public void Begin()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        void IDisposable.Dispose()
        {
            foreach (var key in _storage.Keys)
            {
                var dic = _storage[key] as IDictionary;
                if (dic != null)
                {
                    dic.Clear();
                }
            }

            _storage.Clear();
        }
    }
}
