using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Query.Language.Structure;
using NUnit.Framework;

namespace MongoDB.Query.Language.Parsing
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void Select_statement()
        {
            var subject = new Parser(new Lexer("SELECT f.lastName"));

            var statementList = subject.Parse();
            statementList.Statements.Count.Should().Be(1);
            var statement = statementList.Statements[0];
            statement.Should().BeOfType<SqlSelectStatement>();

            var select = ((SqlSelectStatement)statement).Select;
            select.Expressions.Count.Should().Be(1);
            select.Expressions[0].Should().BeOfType<SqlFieldExpression>();

            var field = (SqlFieldExpression)select.Expressions[0];
            field.Name.Should().Be("lastName");
            field.Expression.Should().BeOfType<SqlCollectionExpression>();

            var collection = (SqlCollectionExpression)field.Expression;
            collection.Name.Should().Be("f");
        }

        private void AssertNext(Lexer lexer, TokenKind tokenKind, string text)
        {
            var token = lexer.Consume();
            token.Kind.Should().Be(tokenKind);
            token.Text.Should().Be(text);
        }
    }
}
