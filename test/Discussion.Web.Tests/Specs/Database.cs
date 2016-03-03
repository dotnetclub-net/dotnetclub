using Jusfr.Persistent.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Discussion.Web.Tests.Specs
{
    public sealed class Database : IDisposable
    {
        string _connectionString;
        public Database()
        {
            //_connectionString = CreateConnectionStringForTesting("127.0.0.1:27017");
            _connectionString = CreateConnectionStringForTesting("192.168.1.178:27017");
            Context = new MongoRepositoryContext(_connectionString);
        }

        public MongoRepositoryContext Context { get; private set; }

        #region Disposing

        ~Database()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            try
            {
                Context.Dispose();
                DropDatabase(_connectionString);
            }
            catch { }
        }

        #endregion

        #region helpers for setting up database

        static string CreateConnectionStringForTesting(string host)
        {
            string connectionString;

            do
            {
                var databaseName = string.Concat("ut_" + DateTime.UtcNow.ToString("yyyyMMddHHMMss") + Guid.NewGuid().ToString("N").Substring(0, 4));
                connectionString = $"mongodb://{host}/{databaseName}";
            }
            while (DatabaseExists(connectionString));
            return connectionString;
        }

        static bool DatabaseExists(string connectionString)
        {
            var mongoUri = new MongoUrl(connectionString);
            var client = new MongoClient(mongoUri);

            var dbList = Enumerate(client.ListDatabases()).Select(db => db.GetValue("name").AsString);
            return dbList.Contains(mongoUri.DatabaseName);
        }

        static IEnumerable<BsonDocument> Enumerate(IAsyncCursor<BsonDocument> docs)
        {
            while (docs.MoveNext())
            {
                foreach (var item in docs.Current)
                {
                    yield return item;
                }
            }
        }

        static void DropDatabase(string connectionString)
        {
            var mongoUri = new MongoUrl(connectionString);
            //new MongoClient(mongoUri).DropDatabase(mongoUri.DatabaseName);
            new MongoClient(mongoUri).DropDatabaseAsync(mongoUri.DatabaseName).Wait();
        }

        #endregion

    }

    // Use shared context to maintain database fixture
    // see https://xunit.github.io/docs/shared-context.html#collection-fixture
    [CollectionDefinition("DbSpec")]
    public class DatabaseCollection : ICollectionFixture<Database>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
