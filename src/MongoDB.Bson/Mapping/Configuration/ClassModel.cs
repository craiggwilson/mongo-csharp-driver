using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Mapping.Configuration
{
    public class ClassModel
    {
        private readonly FieldModelCollection _fieldModels;
        private readonly Type _type;
        private WeightedValue<bool> _runConventions;

        public ClassModel(Type type)
        {
            if (_type == null)
            {
                throw new ArgumentNullException("type");
            }

            _type = type;
            _runConventions = new WeightedValue<bool>(true, Weights.Default);
        }

        public IEnumerable<FieldModel> Fields
        {
            get { return _fieldModels; }
        }

        public bool RunConventions
        {
            get { return _runConventions.Get(); }
        }

        public Type Type
        {
            get { return _type; }
        }

        public FieldModel Map(string name, Type type, int weight)
        {
            return _fieldModels.GetOrCreate(name, type, weight);
        }

        public void SetRunConventions(bool value, int weight)
        {
            _runConventions.Set(value, weight);
        }

        public void Unmap(string name, int weight)
        {
            _fieldModels.Remove(name, weight);
        }
    }
}
