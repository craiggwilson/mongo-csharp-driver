using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Structure
{
    public sealed class SqlSelectStatement : SqlStatement
    {
        private readonly SqlSelectClause _select;

        public SqlSelectStatement(SqlSelectClause select)
        {
            _select = select;
        }

        public SqlSelectClause Select => _select;

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(_select.ToString());
            return builder.ToString();
        }
    }
}