using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Mapping.Configuration
{
    public class FieldModel<TClass, TField>
    {
        private readonly FieldModel _builder;

        internal FieldModel(FieldModel builder)
        {
            _builder = builder;
        }

        public FieldModel<TClass, TField> ElementName(string elementName)
        {
            _builder.SetElementName(elementName, Weights.UserCode);
            return this;
        }
    }
}
