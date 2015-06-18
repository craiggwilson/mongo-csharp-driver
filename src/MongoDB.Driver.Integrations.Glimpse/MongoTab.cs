using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glimpse.Core.Extensions;
using Glimpse.Core.Extensibility;

namespace MongoDB.Driver.Integrations.Glimpse
{
    public class MongoTab : TabBase, IDocumentation, ITabSetup
    {
        public string DocumentationUri => "http://mongodb.github.io/mongo-csharp-driver"; 

        public override string Name => "MongoDB";

        public override RuntimeEvent ExecuteOn => RuntimeEvent.EndRequest;

        public override object GetData(ITabContext context)
        {
            throw new NotImplementedException();
        }

        public void Setup(ITabSetupContext context)
        {

        }

    }
}
