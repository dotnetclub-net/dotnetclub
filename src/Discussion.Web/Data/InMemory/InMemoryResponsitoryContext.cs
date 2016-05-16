using Jusfr.Persistent;
using System;
using System.Collections.Concurrent;
using System.Collections;

namespace Discussion.Web.Data.InMemory
{

    public class InMemoryResponsitoryContext : DisposableObject, IRepositoryContext
    {
        private ConcurrentDictionary<string, object> _storage = new ConcurrentDictionary<string, object>();

        public bool DistributedTransactionSupported { get; } = false;

        public ConcurrentDictionary<TKey, TEntry> GetRepositoryForEntity<TKey, TEntry>()
        {
            var type = typeof(TEntry).FullName;
            var entryStorage = _storage.GetOrAdd(type, typeName =>
            {
                return new ConcurrentDictionary<TKey, TEntry>();
            });

            return entryStorage as ConcurrentDictionary<TKey, TEntry>;
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

        protected override void DisposeManaged()
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
