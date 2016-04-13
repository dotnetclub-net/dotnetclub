using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;

namespace Discussion.Web.Data
{
    public class MongoDbUtils
    {
        public static bool DatabaseExists(string connectionString)
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
    }
}
