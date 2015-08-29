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
        [TestCase(",", TokenKind.Comma, ",")]
        [TestCase("[", TokenKind.LBracket, "[")]
        [TestCase("]", TokenKind.RBracket, "]")]
        [TestCase("{", TokenKind.LBrace, "{")]
        [TestCase("}", TokenKind.RBrace, "}")]
        [TestCase("(", TokenKind.LParen, "(")]
        [TestCase(")", TokenKind.RParen, ")")]
        [TestCase("a", TokenKind.Text, "a")]
        [TestCase("abc", TokenKind.Text, "abc")]
        [TestCase("$_a-b^c_", TokenKind.Text, "$_a-b^c_")]
        [TestCase("\"$_a-b^c_\"", TokenKind.QuotedText, "$_a-b^c_")]
        [TestCase("\"$_a-\"\"b^c_\"", TokenKind.QuotedText, "$_a-\"b^c_")]
        [TestCase("1", TokenKind.Number, "1")]
        [TestCase("123", TokenKind.Number, "123")]
        [TestCase("1.78", TokenKind.Number, "1.78")]
        [TestCase(".79", TokenKind.Number, ".79")]
        [TestCase("-79", TokenKind.Number, "-79")]
        [TestCase("-14.6", TokenKind.Number, "-14.6")]
        [TestCase("-.35", TokenKind.Number, "-.35")]
        public void Single_token(string input, TokenKind kind, string text)
        {
            var subject = new Lexer(input);

            AssertNext(subject, kind, text);
        }

        private void AssertNext(Lexer lexer, TokenKind tokenKind, string text)
        {
            var token = lexer.Consume();
            token.Kind.Should().Be(tokenKind);
            token.Text.Should().Be(text);
        }
    }
}
