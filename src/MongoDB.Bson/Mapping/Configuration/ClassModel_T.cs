using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Mapping.Configuration
{
    public class ClassModel<TClass>
    {
        private readonly ClassModel _builder;

        public ClassModel()
            : this(new ClassModel(typeof(TClass)))
        { }

        internal ClassModel(ClassModel builder)
        {
            _builder = builder;
        }

        public void RunConventions(bool value)
        {
            _builder.SetRunConventions(value, Weights.UserCode);
        }

        public FieldModel<TClass, TField> Map<TField>(string name)
        {
            var fieldModelBuilder = _builder.Map(name, typeof(TField), Weights.UserCode);
            return new FieldModel<TClass, TField>(fieldModelBuilder);
        }
    }
}
