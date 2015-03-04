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

        public Parser(IInputStream<Token> input)
        {
            _input = input;
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
                if(_input.LA(0).Kind == TokenKind.EOF)
                {
                    break;
                }

                list.Add(ReadStage());
            }

            return list;
        }

        private PipelineStage ReadStage()
        {
            var operatorToken = Consume(TokenKind.Word);
            var documentToken = Consume(TokenKind.Document);

            return new PipelineStage(operatorToken.Text, documentToken.Text);
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
