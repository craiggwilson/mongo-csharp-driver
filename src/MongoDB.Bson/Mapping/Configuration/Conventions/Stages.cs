using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Bson.Mapping.Configuration.Conventions
{
    public static class Stages
    {
        public static readonly int Startup = 100;

        public static readonly int MapFields = 200;

        public static readonly int ConfigureFields = 300;

        public static readonly int Finish = 400;
    }
}
