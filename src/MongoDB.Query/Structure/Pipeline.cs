using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Structure
{
    public class Pipeline : Node
    {
        private readonly string _collectionName;
        private readonly IReadOnlyList<PipelineStage> _stages;

        public Pipeline(string collectionName, IEnumerable<PipelineStage> stages)
        {
            _collectionName = collectionName;
            _stages = stages.ToList().AsReadOnly();
        }

        public string CollectionName
        {
            get { return _collectionName; }
        }

        public override NodeKind Kind
        {
            get { return NodeKind.Pipeline; }
        }

        public IReadOnlyList<PipelineStage> Stages
        {
            get { return _stages; }
        }
    }
}
