using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Mapping.Configuration
{
    public static class Weights
    {
        public static readonly int Default = 0;

        public static readonly int BuiltInConvention = 100;

        public static readonly int UserConvention = 200;

        public static readonly int BuiltInAttribute = 300;

        public static readonly int UserAttribute = 400;

        public static readonly int UserCode = 500;
    }
}
