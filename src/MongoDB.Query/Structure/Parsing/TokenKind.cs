using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Structure.Parsing
{
    public enum TokenKind
    {
        Word,
        Asterick,
        LParen,
        RParen,
        Equal,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        EOF
    }
}
