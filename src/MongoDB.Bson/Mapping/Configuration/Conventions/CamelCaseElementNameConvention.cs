using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Mapping.Configuration.Conventions
{
    public class CamelCaseElementNameConvention : IConvention
    {
        public int Stage
        {
            get { return Stages.ConfigureFields; }
        }

        public void Apply(ClassModel model)
        {
            foreach (var field in model.Fields)
            {
                field.SetElementName(field.Name.Substring(0, 1).ToLower() + field.Name.Substring(1), Weights.UserConvention);
            }
        }
    }
}
