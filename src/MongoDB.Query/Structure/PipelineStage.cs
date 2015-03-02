using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Structure
{
    public abstract class PipelineStage : Node
    {
        public abstract string Name { get; }
    }
}
