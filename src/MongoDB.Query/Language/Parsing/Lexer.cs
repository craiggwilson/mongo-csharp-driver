using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Language.Parsing
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

            if (i != count)
            {
                Array.Resize(ref tokens, i + 1);
            }

            return tokens;
        }

        private Token Read()
        {
            ReadWhiteSpace();

            var c = _input.LA(0);
            if (c == '\0')
            {
                return ReadEndOfFile();
            }

            if (char.IsWhiteSpace(c))
            {
                return ReadWhiteSpace();
            }

            switch (c)
            {
                case '{':
                    return CreateToken(TokenKind.LBrace, _input.Consume());
                case '}':
                    return CreateToken(TokenKind.RBrace, _input.Consume());
                case '[':
                    return CreateToken(TokenKind.LBracket, _input.Consume());
                case ']':
                    return CreateToken(TokenKind.RBracket, _input.Consume());
                case '(':
                    return CreateToken(TokenKind.LParen, _input.Consume());
                case ')':
                    return CreateToken(TokenKind.RParen, _input.Consume());
                case ',':
                    return CreateToken(TokenKind.Comma, _input.Consume());
                case '"':
                    return ReadQuotedText();
                default:
                    return ReadTextOrNumberOrDot();
            }
        }

        private Token ReadEndOfFile()
        {
            return CreateToken(TokenKind.EOF, "<<EOF>>");
        }

        private Token ReadNumber()
        {
            _input.Mark();
            var c = _input.Consume(); // number, -, .
            if (c == '-')
            {
                c = _input.Consume(); // ., number
            }
            while (char.IsNumber(_input.LA(0)))
            {
                _input.Consume();
            }

            if (_input.LA(0) == '.')
            {
                _input.Consume();
                while (char.IsNumber(_input.LA(0)))
                {
                    _input.Consume();
                }
            }

            return CreateTokenFromMark(TokenKind.Number);
        }

        private Token ReadQuotedText()
        {
            var c = _input.Consume(); // double quote
            _input.Mark();
            bool hasEscapedQuotes = false;

            ReadQuotedTextWhile:
            while (_input.LA(0) != '\0' && _input.LA(0) != '"')
            {
                _input.Consume();
            }

            if (_input.LA(1) == '"')
            {
                hasEscapedQuotes = true;
                _input.Consume(2);
                // we are escaping the quote...
                goto ReadQuotedTextWhile;
            }

            if (_input.LA(0) == '\0')
            {
                throw new ParseException("Unexpected end of file.");
            }

            var text = new string(_input.ClearMark());
            if (hasEscapedQuotes)
            {
                text = text.Replace("\"\"", "\"");
            }

            _input.Consume(); // end quote

            return CreateToken(TokenKind.QuotedText, text);
        }

        private Token ReadTextOrNumberOrDot()
        {
            char c = _input.LA(0);

            // 123
            if (char.IsNumber(c))
            {
                return ReadNumber();
            }

            // -123
            if (c == '-' && char.IsNumber(_input.LA(1)))
            {
                return ReadNumber();
            }

            // -.123
            if (c == '-' && _input.LA(1) == '.' && char.IsNumber(_input.LA(2)))
            {
                return ReadNumber();
            }

            // .123
            if (c == '.' && char.IsNumber(_input.LA(1)))
            {
                return ReadNumber();
            }

            // .Text
            if (c == '.')
            {
                return CreateToken(TokenKind.Dot, _input.Consume());
            }

            return ReadText();
        }

        private Token ReadText()
        {
            _input.Mark();

            ReadTextSwitch:
            switch (_input.LA(0))
            {
                // terminals
                case ',':
                case '.':
                case '[':
                case ']':
                case '{':
                case '}':
                case '(':
                case ')':
                case '\0':
                    break;
                default:
                    _input.Consume();
                    goto ReadTextSwitch;
            }

            return CreateTokenFromMark(TokenKind.Text);
        }

        private Token ReadWhiteSpace()
        {
            _input.Mark();
            while (char.IsWhiteSpace(_input.LA(0)))
            {
                _input.Consume();
            }
            return CreateTokenFromMark(TokenKind.Whitespace);
        }

        private Token CreateToken(TokenKind kind, char c)
        {
            return new Token(kind, c);
        }

        private Token CreateToken(TokenKind kind, string text)
        {
            return new Token(kind, text);
        }

        private Token CreateTokenFromMark(TokenKind kind)
        {
            return new Token(kind, new string(_input.ClearMark()));
        }
    }
}