using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Structure
{
    public class SqlFieldOrCollectionExpression : SqlExpression
    {
        private readonly string _name;

        public SqlFieldOrCollectionExpression(string name)
        {
            _name = name;
        }

        public string Name => _name;

        public override string ToString() => _name;
    }
}
