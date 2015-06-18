using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Integrations.Glimpse
{
    public static class ClusterBuilderExtensions
    {
        public static ClusterBuilder UseGlimpse(this ClusterBuilder builder)
        {
            Ensure.IsNotNull(builder, "builder");

            return builder.Subscribe(new GlimpseEventSubscriber());
        }
    }
}
