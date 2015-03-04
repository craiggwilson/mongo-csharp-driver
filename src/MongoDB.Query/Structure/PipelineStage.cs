using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Structure
{
    public class PipelineStage : Node
    {
        private readonly string _document;
        private readonly string _pipelineOperator;

        public PipelineStage(string pipelineOperator, string document)
        {
            _pipelineOperator = pipelineOperator;
            _document = document;
        }

        public string Document
        {
            get { return _document; }
        }

        public override NodeKind Kind
        {
            get { return NodeKind.PipelineStage; }
        }

        public string PipelineOperator
        {
            get { return _pipelineOperator; }
        }


    }
}
