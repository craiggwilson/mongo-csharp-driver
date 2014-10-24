/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a serializer for a CommandResult.
    /// </summary>
    /// <typeparam name="TDocument">
    /// </typeparam>
    public class AggregateResultSerializer<TDocument> : SerializerBase<AggregateResult<TDocument>>
    {
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <returns>The value.</returns>
        public override AggregateResult<TDocument> Deserialize(BsonDeserializationContext context)
        {
            var response = new BsonDocument();
            IEnumerable<TDocument> results = null;
            
            var bsonReader = context.Reader;
            bsonReader.ReadStartDocument();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();
                if (name == "result")
                {
                    results = ReadResults(context);
                    continue;
                }

                if (name == "cursor")
                {
                    bsonReader.ReadStartDocument();
                    response.Add("cursor", new BsonDocument("id", bsonReader.ReadInt64("id")));
                    bsonReader.ReadString("ns");

                    results = ReadResults(context);
                    bsonReader.ReadEndDocument();

                    continue;
                }

                var value = context.DeserializeWithChildContext(BsonValueSerializer.Instance);
                response.Add(name, value);
            }

            bsonReader.ReadEndDocument();

            return
                (AggregateResult<TDocument>)
                Activator.CreateInstance(typeof(AggregateResult<TDocument>), new object[] { response, results });
        }

        // private methods
        private IEnumerable<TDocument> ReadResults(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;
            var values = new List<TDocument>();

            var resultSerializer = BsonSerializer.LookupSerializer<TDocument>();

            bsonReader.ReadStartArray();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                values.Add(context.DeserializeWithChildContext(resultSerializer));
            }

            bsonReader.ReadEndArray();

            return values;
        }
    }
}
