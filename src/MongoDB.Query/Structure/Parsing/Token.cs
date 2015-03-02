using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Structure.Parsing
{
    public class Token
    {
        public TokenKind Kind { get; private set; }

        public string Text { get; private set; }

        public Token(TokenKind kind, char characater)
            : this(kind, characater.ToString())
        { }

        public Token(TokenKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }
    }
}
