using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Mapping.Configuration
{
    public interface IConvention
    {
        int Stage { get; }

        void Apply(ClassModel builder);
    }
}
