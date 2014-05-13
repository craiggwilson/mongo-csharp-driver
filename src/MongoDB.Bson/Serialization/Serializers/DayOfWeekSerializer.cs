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
    public class DayOfWeekSerializer : BsonBaseSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DayOfWeekSerializer" /> class.
        /// </summary>
        public DayOfWeekSerializer()
            : base(new RepresentationSerializationOptions(BsonType.Int32))
        { 
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>
        /// An object.
        /// </returns>
        /// <exception cref="System.IO.FileFormatException"></exception>
        public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        {
            VerifyTypes(nominalType, actualType, typeof(DayOfWeek));

            BsonType bsonType = bsonReader.GetCurrentBsonType();
            switch (bsonType)
            {
                case BsonType.Int32:
                    return (DayOfWeek)(bsonReader.ReadInt32() - 1);
                case BsonType.String:
                    return (DayOfWeek)Enum.Parse(typeof(DayOfWeek), bsonReader.ReadString());
                default:
                    var message = string.Format("Cannot deserialize DayOfWeek from BsonType {0}.", bsonType);
                    throw new FileFormatException(message);
            }
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        /// <exception cref="BsonSerializationException"></exception>
        public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            var dayOfWeekValue = (DayOfWeek)value;
            var representationSerializationOptions = EnsureSerializationOptions<RepresentationSerializationOptions>(options);

            switch (representationSerializationOptions.Representation)
            {
                case BsonType.Int32:
                    bsonWriter.WriteInt32(((int)dayOfWeekValue) + 1);
                    break;
                case BsonType.String:
                    bsonWriter.WriteString(dayOfWeekValue.ToString());
                    break;
                default:
                    var message = string.Format("'{0}' is not a valid DayOfWeek representation.", representationSerializationOptions.Representation);
                    throw new BsonSerializationException(message);
            }
        }
    }
}