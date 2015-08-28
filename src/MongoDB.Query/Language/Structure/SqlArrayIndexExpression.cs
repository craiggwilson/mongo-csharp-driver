using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Structure
{
    public class SqlArrayIndexExpression : SqlExpression
    {
        private readonly SqlExpression _expression;
        private readonly string _number;

        public SqlArrayIndexExpression(SqlExpression expression, string number)
        {
            _expression = expression;
            _number = number;
        }

        public SqlExpression Expression => _expression;

        public string Number => _number;

        public override string ToString() => $"{_expression.ToString()}[{_number}]";
    }
}
