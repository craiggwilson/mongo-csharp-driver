using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Structure
{
    public sealed class SqlDocumentExpression : SqlExpression
    {
        private readonly IReadOnlyList<SqlAliasExpression> _expressions;

        public SqlDocumentExpression(IEnumerable<SqlAliasExpression> expressions)
        {
            _expressions = expressions as IReadOnlyList<SqlAliasExpression>;
            if (_expressions == null)
            {
                _expressions = expressions.ToList().AsReadOnly();
            }
        }

        public IReadOnlyList<SqlAliasExpression> Expressions => _expressions;

        public override string ToString()
        {
            return string.Format("{{{0}}}",
                string.Join(", ",
                    _expressions.Select(x => string.Format("\"{0}\": {1}", x.Name, x.Expression))));
        }
    }
}
