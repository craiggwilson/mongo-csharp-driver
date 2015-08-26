using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Structure
{
    public class SqlConstantExpression : SqlExpression
    {
        private readonly SqlType _type;
        private readonly string _value;

        public SqlConstantExpression(SqlType type, string value)
        {
            _type = type;
            _value = value;
        }

        public SqlType Type => _type;
        public string Value => _value;

        public override string ToString() => _value;
    }
}
