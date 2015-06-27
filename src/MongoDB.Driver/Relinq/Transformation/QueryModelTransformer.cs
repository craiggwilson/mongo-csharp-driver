using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq;

namespace MongoDB.Driver.Relinq.Transformation
{
    public class QueryModelTransformer : QueryModelVisitorBase
    {
        public static void Transform(QueryModel queryModel)
        {
            var transformer = new QueryModelTransformer();

            queryModel.Accept(transformer);
        }

        public override void VisitMainFromClause(Remotion.Linq.Clauses.MainFromClause fromClause, QueryModel queryModel)
        {
            base.VisitMainFromClause(fromClause, queryModel);
        }
    }
}