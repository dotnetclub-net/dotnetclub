using Jusfr.Persistent.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discussion.Web.Tests.Specs
{
    public class DbSpecBase: IDisposable
    {
        string _connectionString;
        public DbSpecBase()
        {
            // _connectionString = CreateConnectionStringForTesting("127.0.0.1:27017");
            _connectionString = CreateConnectionStringForTesting("192.168.1.178:27017");
            DbContext = new MongoRepositoryContext(_connectionString);
        }


        public MongoRepositoryContext DbContext { get; private set; }


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

        void IDisposable.Dispose()
        {
            var mongoUri = new MongoUrl(_connectionString);
            var client = new MongoClient(mongoUri);
            client.DropDatabase(mongoUri.DatabaseName);
        }
    }
}
