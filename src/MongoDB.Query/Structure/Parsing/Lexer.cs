using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Structure.Parsing
{
    public class Lexer : AbstractBufferedInputStream<Token>
    {
        private readonly IInputStream<char> _input;

        public Lexer(string input)
            : this(new BufferedCharInputStream(input, 100))
        { }

        public Lexer(IInputStream<char> input)
            : base(100)
        {
            _input = input;
        }

        protected override Token[] ReadInput(int count)
        {
            var tokens = new Token[count];
            int i;
            for (i = 0; i < count; i++)
            {
                tokens[i] = Read();
                if (tokens[i].Kind == TokenKind.EOF)
                {
                    break;
                }
            }

            if(i != count)
            {
                Array.Resize(ref tokens, i + 1);
            }

            return tokens;
        }

        private Token Read()
        {
            ReadWhiteSpace();

            var c = _input.LA(0);
            if(c == '\0')
            {
                return ReadEndOfFile();
            }

            switch(c)
            {
                case '(':
                    return new Token(TokenKind.LParen, c);
                case ')':
                    return new Token(TokenKind.RParen, c);
                case '*':
                    return new Token(TokenKind.Asterick, c);
                case '=':

            }
        }

        private Token ReadEndOfFile()
        {
            return new Token(TokenKind.EOF, null);
        }

        private Token ReadTerminal()
        {

        }

        private void ReadWhiteSpace()
        {
            var c = _input.LA(0);
            while (char.IsWhiteSpace(c))
            {
                _input.Consume();
                c = _input.LA(0);
            }
        }
    }
}