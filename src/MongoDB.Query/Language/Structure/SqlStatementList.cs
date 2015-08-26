using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Structure
{
    public class SqlStatementList
    {
        private readonly IReadOnlyList<SqlStatement> _statements;

        public SqlStatementList(IEnumerable<SqlStatement> statements)
        {
            _statements = statements as IReadOnlyList<SqlStatement>;
            if (_statements == null)
            {
                _statements = statements.ToList().AsReadOnly();
            }
        }

        public IReadOnlyList<SqlStatement> Statements => _statements;

        public override string ToString() =>
            string.Join(Environment.NewLine + Environment.NewLine, _statements.Select(x => x.ToString()));
    }
}