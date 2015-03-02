using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Structure
{
    public abstract class Node
    {
        protected Node(LineInfo lineInfo)
        {
            LineInfo = lineInfo;
        }

        public abstract NodeKind Kind { get; }

        public LineInfo LineInfo { get; private set; }
    }
}
