using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using MongoDB.Bson;

namespace MongoDB.Driver.TestWebApplication.Controllers
{
    public class ValuesController : ApiController
    {
        private static readonly MongoClient _client = new MongoClient("mongodb://localhost/maxPoolSize=500&minPoolSize=50");
        private static readonly IMongoDatabase _database = _client.GetDatabase("test");
        private static readonly IMongoCollection<BsonDocument> _collection = _database.GetCollection<BsonDocument>("values");

        // GET api/values
        public async Task<IEnumerable<string>> Get()
        {
            var values = await _collection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();
            return values.Select(x => (string)x["value"]);
        }
    }
}
