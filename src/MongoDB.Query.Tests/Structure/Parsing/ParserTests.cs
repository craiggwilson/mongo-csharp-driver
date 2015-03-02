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
        public void Minimal_with_project_asterick()
        {
            var subject = new Parser(new Lexer("FROM a PROJECT *"));

            var pipeline = subject.Parse();
            pipeline.CollectionName.Should().Be("a");
            pipeline.Stages.Should().HaveCount(1);
            pipeline.Stages[0].Name.Should().Be("PROJECT");
            ((ProjectPipelineStage)pipeline.Stages[0]).Nodes.Should().HaveCount(0);
        }

        private void AssertNext(Lexer lexer, TokenKind tokenKind, string text)
        {
            var token = lexer.Consume();
            token.Kind.Should().Be(tokenKind);
            token.Text.Should().Be(text);
        }
    }
}
