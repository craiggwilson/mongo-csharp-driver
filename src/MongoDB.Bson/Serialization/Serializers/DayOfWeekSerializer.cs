using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Serializes the DayOfWeek enumeration.
    /// </summary>
    public class DayOfWeekSerializer : EnumSerializer<DayOfWeek>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DayOfWeekSerializer" /> class.
        /// </summary>
        public DayOfWeekSerializer()
            : base(BsonType.Int32, 1)
        { 
        }
    }
}