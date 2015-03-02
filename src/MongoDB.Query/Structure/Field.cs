using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Query.Structure
{
    public class Field : Node
    {
        private readonly string _name;

        public Field(string name)
        {
            _name = name;
        }

        public override NodeKind Kind
        {
            get { return NodeKind.Field; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}