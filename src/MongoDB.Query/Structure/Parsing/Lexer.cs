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
                case '{':
                    return ReadDocument();
                default:
                    return ReadWord();
            }
        }

        private Token ReadEndOfFile()
        {
            return new Token(TokenKind.EOF, "<<EOF>>");
        }

        private Token ReadDocument()
        {
            _input.Mark();
            int count = 0;
            while(true)
            {
                // TODO: Need to account for opening and closing 
                // braces inside quotes as well as escaped variants 
                var c = _input.Consume();
                if(c == '{')
                {
                    count++;
                }
                else if (c == '}')
                {
                    count--;
                    if(count == 0)
                    {
                        break;
                    }
                }
                else if (c == '\0')
                {
                    throw ParseException.Create("Unexpected EOF.");
                }
            }
            return new Token(TokenKind.Document, new string(_input.ClearMark()));
        }

        private Token ReadWord()
        {
            _input.Mark();
            while (char.IsLetterOrDigit(_input.LA(0)))
            {
                _input.Consume();
            }
            return new Token(TokenKind.Word, new string(_input.ClearMark()));
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