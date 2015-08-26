using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Query.Language.Structure;

namespace MongoDB.Query.Language.Parsing
{
    public class Parser
    {
        private readonly IInputStream<Token> _input;

        public Parser(IInputStream<Token> input)
        {
            _input = input;
        }

        public SqlStatementList Parse()
        {
            return ReadStatementList();
        }

        private SqlStatementList ReadStatementList()
        {
            var statements = new List<SqlStatement>();

            while (_input.LA(0).Kind != TokenKind.EOF)
            {
                statements.Add(ReadStatement());
            }

            return new SqlStatementList(statements);
        }

        private SqlStatement ReadStatement()
        {
            ConsumeWhiteSpace();

            var token = _input.LA(0);
            if (token.Kind != TokenKind.Text)
            {
                throw new ParseException($"Expected {TokenKind.Text} but found {token.Kind}");
            }

            switch (token.Text.ToUpperInvariant())
            {
                case "SELECT":
                    return ReadSelectStatement();
            }

            throw new ParseException($"Unknown statement {token.Text}.");
        }

        private SqlSelectStatement ReadSelectStatement()
        {
            var select = ReadSelectClause();

            return new SqlSelectStatement(select);
        }

        private SqlSelectClause ReadSelectClause()
        {
            var select = ConsumeText("SELECT");
            ConsumeWhiteSpace();

            var expressions = ReadExpressionList();

            return new SqlSelectClause(expressions);
        }

        private List<SqlExpression> ReadExpressionList()
        {
            var expressions = new List<SqlExpression>();
            while (_input.LA(0).Kind != TokenKind.EOF)
            {
                expressions.Add(ReadExpression());
                ConsumeWhiteSpace();
                if (_input.LA(0).Kind != TokenKind.Comma)
                {
                    break;
                }
                Consume(); // comma
            }

            return expressions;
        }

        private SqlExpression ReadExpression()
        {
            switch (_input.LA(0).Kind)
            {
                case TokenKind.Text:
                    return ReadFieldExpression();
            }

            throw new ParseException($"Unknown expression {_input.LA(0).Text}");
        }

        private SqlFieldExpression ReadFieldExpression()
        {
            SqlExpression current = ReadCollectionExpression();

            var token = _input.LA(0);

            while (token.Kind == TokenKind.Dot)
            {
                Consume(TokenKind.Dot);
                token = Consume(TokenKind.Text);
                current = new SqlFieldExpression(current, token.Text);
                token = _input.LA(0);
            }

            return (SqlFieldExpression)current;
        }

        private SqlCollectionExpression ReadCollectionExpression()
        {
            var token = Consume(TokenKind.Text);
            return new SqlCollectionExpression(token.Text);
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

        private Token Consume(TokenKind kind)
        {
            var token = Consume();
            if (token.Kind != kind)
            {
                throw new ParseException($"Expected {kind} but found {token.Kind}.");
            }

            return token;
        }

        private Token Consume()
        {
            return _input.Consume();
        }

        private void ConsumeWhiteSpace()
        {
            while (_input.LA(0).Kind == TokenKind.Whitespace)
            {
                _input.Consume();
            }
        }
    }
}
