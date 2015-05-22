using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Mapping.Configuration
{
    public class FieldModel
    {
        private readonly string _name;
        private readonly Type _type;
        private readonly WeightedValue<string> _elementName;

        public FieldModel(string name, Type type)
        {
            _name = name;
            _type = type;
            _elementName = new WeightedValue<string>(_name, Weights.Default);
        }

        public string Name
        {
            get { return _name; }
        }

        public Type Type
        {
            get { return _type; }
        }

        public string ElementName
        {
            get { return _elementName.Get(); }
        }

        public void SetElementName(string elementName, int weight)
        {
            _elementName.Set(elementName, weight);
        }
    }
}
