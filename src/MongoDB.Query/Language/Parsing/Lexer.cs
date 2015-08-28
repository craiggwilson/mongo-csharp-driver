﻿using System;
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
                case '.':
                    return CreateToken(TokenKind.Dot, _input.Consume());
                case ',':
                    return CreateToken(TokenKind.Comma, _input.Consume());
                case '"':
                    return ReadQuotedText();
                default:
                    return ReadWord();
            }
        }

        private Token ReadEndOfFile()
        {
            return new Token(TokenKind.EOF, "<<EOF>>");
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

            _input.Consume(); // end quote
            var text = new string(_input.ClearMark());
            if (hasEscapedQuotes)
            {
                text = text.Replace("\"\"", "\"");
            }
            return new Token(TokenKind.QuotedText, text);
        }

        private Token CreateToken(TokenKind kind, char c)
        {
            return new Token(kind, c);
        }

        private Token ReadWord()
        {
            _input.Mark();
            while (char.IsLetterOrDigit(_input.LA(0)))
            {
                _input.Consume();
            }
            return new Token(TokenKind.Text, new string(_input.ClearMark()));
        }

        private Token ReadWhiteSpace()
        {
            _input.Mark();
            while (char.IsWhiteSpace(_input.LA(0)))
            {
                _input.Consume();
            }
            return new Token(TokenKind.Whitespace, new string(_input.ClearMark()));
        }
    }
}