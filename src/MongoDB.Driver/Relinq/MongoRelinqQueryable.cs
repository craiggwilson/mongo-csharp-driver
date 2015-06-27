using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace MongoDB.Driver.Relinq
{
    internal class MongoRelinqQueryable<T> : QueryableBase<T>
    {
        public MongoRelinqQueryable(IQueryParser queryParser, IQueryExecutor queryExecutor)
            : base(new DefaultQueryProvider(typeof(MongoRelinqQueryable<>), queryParser, queryExecutor))
        {
        }

        public MongoRelinqQueryable(IQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
        }
    }
}
