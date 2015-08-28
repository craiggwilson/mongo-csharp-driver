using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Parsing
{
    public enum TokenKind
    {
        EOF,
        Whitespace,
        Text,
        QuotedText,
        Number,
        LBrace,
        RBrace,
        LBracket,
        RBracket,
        Dot,
        Comma
    }
}
