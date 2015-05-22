using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Mapping.Configuration.Conventions
{
    public class PublicReadWritePropertyClassModelConvention : IConvention
    {
        public int Stage
        {
            get { return Stages.MapFields; }
        }

        public void Apply(ClassModel model)
        {
            foreach (var property in model.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                model.Map(property.Name, property.PropertyType, Weights.BuiltInConvention);
            }
        }
    }
}
