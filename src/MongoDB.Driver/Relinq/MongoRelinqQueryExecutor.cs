using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Relinq.Transformation;
using MongoDB.Driver.Relinq.Translation;
using Remotion.Linq;

namespace MongoDB.Driver.Relinq
{
    internal class MongoRelinqQueryExecutor<T> : IQueryExecutor
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRelinqQueryExecutor(IMongoCollection<T> collection)
        {
            _collection = collection;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var mongoQueryModel = PrepareQuery(queryModel);
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            throw new NotImplementedException();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            throw new NotImplementedException();
        }

        private MongoQueryModel PrepareQuery(QueryModel queryModel)
        {
            QueryModelTransformer.Transform(queryModel);
            return QueryModelTranslator.Translate(queryModel);
        }
    }
}
