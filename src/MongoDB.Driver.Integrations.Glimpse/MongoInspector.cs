using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glimpse.Core.Extensibility;
using MongoDB.Driver.Core.Configuration;

namespace MongoDB.Driver.Integrations.Glimpse
{
    public class MongoInspector : IInspector
    {
        public void Setup(IInspectorContext context)
        {
            GlimpseEventSubscriber.Initialize(context.MessageBroker, context.TimerStrategy);
        }
    }
}
