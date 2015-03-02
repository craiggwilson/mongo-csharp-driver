using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Structure.Parsing
{
    public class Parser
    {
        private readonly IInputStream<Token> _input;
        private readonly Dictionary<string, Func<PipelineStage>> _stageParsers;

        public Parser(IInputStream<Token> input)
        {
            _input = input;
            _stageParsers = new Dictionary<string, Func<PipelineStage>>(StringComparer.OrdinalIgnoreCase)
            {
                { "PROJECT", ReadProjectStage },
                { "MATCH", ReadMatchStage },
            };
        }

        public Pipeline Parse()
        {
            return ReadPipeline();
        }

        private Pipeline ReadPipeline()
        {
            var from = ConsumeWord("FROM");

            var collectionName = Consume(TokenKind.Word);
            var stages = ReadPipelineStages();
            return new Pipeline(collectionName.Text, stages);
        }

        private IEnumerable<PipelineStage> ReadPipelineStages()
        {
            if(_input.LA(0).Kind == TokenKind.EOF)
            {
                return Enumerable.Empty<PipelineStage>();
            }

            var list = new List<PipelineStage>();
            while (true)
            {
                var token = Consume();
                if(token.Kind == TokenKind.EOF)
                {
                    break;
                }

                Func<PipelineStage> stageParser;
                if (!_stageParsers.TryGetValue(token.Text, out stageParser))
                {
                    throw ParseException.Create("Expected a pipeline operator, but found {0}.", token.Text);
                }

                list.Add(stageParser());
            }

            return list;
        }

        private ProjectPipelineStage ReadMatchStage()
        {
            var token = Consume();

            if(token.Kind == TokenKind.Word)
            {
                ReadPredicate();
            }

            throw new NotSupportedException();
        }

        private ProjectPipelineStage ReadProjectStage()
        {
            var token = Consume();
            if(token.Kind == TokenKind.Asterick)
            {
                return new ProjectPipelineStage(Enumerable.Empty<Node>());
            }

            throw new NotSupportedException();
        }

        private Token ConsumeWord(string text)
        {
            var token = Consume(TokenKind.Word);
            if (!token.Text.Equals(text, StringComparison.OrdinalIgnoreCase))
            {
                ParseException.Create("Expected {0}, but found {1}.", text, token.Text);
            }
            return token;
        }

        private Token Consume(TokenKind kind)
        {
            var token = Consume();
            if (token.Kind != kind)
            {
                throw ParseException.Create("Unexpected token {0}.", token.Text);
            }

            return token;
        }

        private Token Consume()
        {
            return _input.Consume();
        }
    }
}
