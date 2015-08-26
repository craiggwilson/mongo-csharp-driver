using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Structure
{
    public sealed class SqlArrayExpression : SqlExpression
    {
        private readonly IReadOnlyList<SqlExpression> _expressions;

        public SqlArrayExpression(IEnumerable<SqlExpression> expressions)
        {
            _expressions = expressions as IReadOnlyList<SqlExpression>;
            if (_expressions == null)
            {
                _expressions = expressions.ToList().AsReadOnly();
            }
        }

        public IReadOnlyList<SqlExpression> Expressions => _expressions;

        public override string ToString() => string.Format("[{0}]", string.Join(", ", _expressions.Select(x => x.ToString())));
    }
}