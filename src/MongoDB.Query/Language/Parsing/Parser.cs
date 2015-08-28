using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Query.Language.Structure;

namespace MongoDB.Query.Language.Parsing
{
    public sealed class Parser
    {
        private readonly IInputStream<Token> _input;

        public Parser(IInputStream<Token> input)
        {
            _input = input;
        }

        public SqlStatementList Parse()
        {
            var statements = new List<SqlStatement>();

            while (LA(0).Kind != TokenKind.EOF)
            {
                statements.Add(ParseStatement());
            }

            return new SqlStatementList(statements);
        }

        internal SqlStatement ParseStatement()
        {
            ConsumeWhiteSpace();

            var token = LA(0);
            if (token.Kind != TokenKind.Text)
            {
                throw new ParseException($"Expected {TokenKind.Text} but found {token.Kind}");
            }

            switch (token.Text.ToUpperInvariant())
            {
                case "SELECT":
                    return ParseSelectStatement();
            }

            throw new ParseException($"Unknown statement {token.Text}.");
        }

        internal SqlSelectStatement ParseSelectStatement()
        {
            var select = ParseSelectClause();

            return new SqlSelectStatement(select);
        }

        internal SqlSelectClause ParseSelectClause()
        {
            var select = ConsumeText("SELECT");
            ConsumeWhiteSpace();

            var expressions = ReadExpressionList();

            return new SqlSelectClause(expressions);
        }

        private List<SqlExpression> ReadExpressionList()
        {
            var expressions = new List<SqlExpression>();
            while (LA(0).Kind != TokenKind.EOF)
            {
                expressions.Add(ReadExpression());
                ConsumeWhiteSpace();
                if (LA(0).Kind != TokenKind.Comma)
                {
                    break;
                }
                Consume(); // comma
            }

            return expressions;
        }

        private SqlExpression ReadExpression()
        {
            switch (LA(0).Kind)
            {
                case TokenKind.Text:
                    return ReadFieldExpression();
            }

            throw new ParseException($"Unknown expression {LA(0).Text}");
        }

        private SqlExpression ReadFieldExpression()
        {
            var token = Consume(TokenKind.Text);

            // we don't know yet whether the first guy in this list is a field off
            // the ambient collection, or an actual collection reference.
            SqlExpression current = new SqlFieldOrCollectionExpression(token.Text);

            ReadFieldExpressionSwitch:
            switch (LA(0).Kind)
            {
                case TokenKind.Dot:
                    Consume(TokenKind.Dot);
                    token = Consume(TokenKind.Text);
                    current = new SqlFieldExpression(current, token.Text);
                    goto ReadFieldExpressionSwitch;
                case TokenKind.LBracket:
                    Consume(TokenKind.LBracket);
                    switch (LA(0).Kind)
                    {
                        case TokenKind.Number:
                            token = Consume(TokenKind.Number);
                            current = new SqlArrayIndexExpression(current, token.Text);
                            Consume(TokenKind.RBracket);
                            goto ReadFieldExpressionSwitch;
                        case TokenKind.QuotedText:
                            token = Consume(TokenKind.QuotedText);
                            current = new SqlFieldExpression(current, token.Text);
                            Consume(TokenKind.RBracket);
                            goto ReadFieldExpressionSwitch;
                        default:
                            throw new ParseException($"Expected number of quoted string, but found {LA(0).Kind}.");
                    }
            }

            while (LA(0).Kind == TokenKind.Dot)
            {

                token = LA(0);
            }

            return current;
        }

        private Token Consume()
        {
            return _input.Consume();
        }

        private Token Consume(TokenKind kind)
        {
            var token = Consume();
            if (token.Kind != kind)
            {
                throw new ParseException($"Expected {kind} but found {token.Kind}.");
            }

            return token;
        }

        private Token ConsumeText(string text)
        {
            var token = Consume(TokenKind.Text);
            if (!token.Text.Equals(text, StringComparison.OrdinalIgnoreCase))
            {
                throw new ParseException($"Expected {text}, but found {token.Text}.");
            }
            return token;
        }

        private void ConsumeWhiteSpace()
        {
            while (LA(0).Kind == TokenKind.Whitespace)
            {
                Consume();
            }
        }

        private Token LA(int count)
        {
            return _input.LA(count);
        }
    }
}
