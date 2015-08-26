using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Structure
{
    public sealed class SqlAliasExpression : SqlExpression
    {
        private readonly SqlExpression _expression;
        private readonly string _name;

        public SqlAliasExpression(SqlExpression expression, string name)
        {
            _expression = expression;
            _name = name;
        }

        public SqlExpression Expression => _expression;
        public string Name => _name;

        public override string ToString() => $"{_expression.ToString()} AS {_name}";
    }
}
