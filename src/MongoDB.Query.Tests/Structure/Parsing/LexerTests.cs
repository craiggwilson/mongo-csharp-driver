using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace MongoDB.Query.Language.Parsing
{
    [TestFixture]
    public class LexerTests
    {
        [Test]
        public void Minimal()
        {
            var subject = new Lexer("FROM a");

            AssertNext(subject, TokenKind.Text, "FROM");
            AssertNext(subject, TokenKind.Whitespace, " ");
            AssertNext(subject, TokenKind.Text, "a");
        }

        private void AssertNext(Lexer lexer, TokenKind tokenKind, string text)
        {
            var token = lexer.Consume();
            token.Kind.Should().Be(tokenKind);
            token.Text.Should().Be(text);
        }
    }
}
