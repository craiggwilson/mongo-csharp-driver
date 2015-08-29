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
        public void Select_clause_with_unanchored_fields()
        {
            var subject = new Parser(new Lexer(@"SELECT lastName, address.state, address[""city""]"));

            var select = subject.ParseSelectClause();
            select.Expressions.Count.Should().Be(3);

            AssertFieldPath("lastName", select.Expressions[0]);
            AssertFieldPath("address.state", select.Expressions[1]);
            AssertFieldPath("address.city", select.Expressions[2]);
        }

        [Test]
        public void Select_clause_with_anchored_fields()
        {
            var subject = new Parser(new Lexer(@"SELECT f.lastName, f.address.state, f[""address""][""city""]"));

            var select = subject.ParseSelectClause();
            select.Expressions.Count.Should().Be(3);

            AssertFieldPath("f.lastName", select.Expressions[0]);
            AssertFieldPath("f.address.state", select.Expressions[1]);
            AssertFieldPath("f.address.city", select.Expressions[2]);
        }

        private void AssertFieldPath(string expectedPath, SqlExpression expression)
        {
            Stack<string> parts = new Stack<string>();
            var current = expression;
            while (current != null)
            {
                var field = current as SqlFieldExpression;
                if (field != null)
                {
                    parts.Push(field.Name);
                    current = field.Expression;
                    continue;
                }

                var fieldOrCollection = current as SqlFieldOrCollectionExpression;
                if (fieldOrCollection != null)
                {
                    parts.Push(fieldOrCollection.Name);
                    break;
                }

                var collection = current as SqlCollectionExpression;
                if (collection != null)
                {
                    parts.Push(collection.Name);
                    break;
                }

                Assert.Fail($"Expected SqlFieldExpression or SqlFieldOrCollectionExpression, but found {current.GetType()}.");
            }

            var path = string.Join(".", parts);
            path.Should().Be(expectedPath);
        }

        private void AssertNext(Lexer lexer, TokenKind tokenKind, string text)
        {
            var token = lexer.Consume();
            token.Kind.Should().Be(tokenKind);
            token.Text.Should().Be(text);
        }
    }
}
