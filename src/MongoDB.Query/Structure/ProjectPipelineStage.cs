using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Structure
{
    public class ProjectPipelineStage : PipelineStage
    {
        private readonly IReadOnlyList<Node> _nodes;

        public ProjectPipelineStage(IEnumerable<Node> nodes)
        {
            _nodes = nodes.ToList().AsReadOnly();
        }

        public override NodeKind Kind
        {
            get { return NodeKind.ProjectStage; }
        }

        public override string Name
        {
            get { return "PROJECT"; }
        }

        public IReadOnlyList<Node> Nodes
        {
            get { return _nodes; }
        }
    }
}
