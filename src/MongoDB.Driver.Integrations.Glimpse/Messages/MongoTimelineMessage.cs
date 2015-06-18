using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glimpse.Core.Message;

namespace MongoDB.Driver.Integrations.Glimpse.Messages
{
    public class MongoTimelineMessage : MessageBase, ITimelineMessage
    {
        internal static TimelineCategoryItem MongoDBTimelineCategory = new TimelineCategoryItem("MongoDB", "#449649", "#52A450");

        public TimelineCategoryItem EventCategory { get; set; }

        public string EventName { get; set; }

        public string EventSubText { get; set; }

        public TimeSpan Duration { get; set; }

        public TimeSpan Offset { get; set; }

        public DateTime StartTime { get; set; }
    }
}
