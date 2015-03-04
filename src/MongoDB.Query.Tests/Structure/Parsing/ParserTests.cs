using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace MongoDB.Query.Structure.Parsing
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void Minimal()
        {
            var subject = new Parser(new Lexer("FROM a"));

            var pipeline = subject.Parse();
            pipeline.CollectionName.Should().Be("a");
            pipeline.Stages.Should().HaveCount(0);
        }

        [Test]
        public void Minimal_with_project()
        {
            var subject = new Parser(new Lexer("FROM a PROJECT {x:1, y:1}"));

            var pipeline = subject.Parse();
            pipeline.CollectionName.Should().Be("a");
            pipeline.Stages.Should().HaveCount(1);
            pipeline.Stages[0].Document.Should().Be("{x:1, y:1}");
        }

        [Test]
        public void Minimal_with_match()
        {
            var subject = new Parser(new Lexer("FROM a MATCH {x:1, y:1}"));

            var pipeline = subject.Parse();
            pipeline.CollectionName.Should().Be("a");
            pipeline.Stages.Should().HaveCount(1);
            pipeline.Stages[0].Document.Should().Be("{x:1, y:1}");
        }

        [Test]
        public void Match_and_project()
        {
            var subject = new Parser(new Lexer("FROM a MATCH {x:1, y:1} PROJECT {x:1}"));

            var pipeline = subject.Parse();
            pipeline.CollectionName.Should().Be("a");
            pipeline.Stages.Should().HaveCount(2);
            pipeline.Stages[0].Document.Should().Be("{x:1, y:1}");
            pipeline.Stages[1].Document.Should().Be("{x:1}");
        }

        private void AssertNext(Lexer lexer, TokenKind tokenKind, string text)
        {
            var token = lexer.Consume();
            token.Kind.Should().Be(tokenKind);
            token.Text.Should().Be(text);
        }
    }
}
