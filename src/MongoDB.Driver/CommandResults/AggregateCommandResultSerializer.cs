/* Copyright 2010-2013 10gen Inc.
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
using System.Collections.Generic;

namespace MongoDB.Driver
{
    using System.Collections;

    /// <summary>
    /// Represents a serializer for a AggregateCommandResult with values of type TValue.
    /// </summary>
    public class AggregateCommandResultSerializer<TDocument> : ClassSerializerBase<AggregateResult<TDocument>>
    {
        // private fields
        private readonly IBsonSerializer _resultSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateCommandResultSerializer{TDocument}" /> class.
        /// </summary>
        /// <param name="resultSerializer">The result serializer.</param>
        public AggregateCommandResultSerializer(IBsonSerializer resultSerializer)
        {
            _resultSerializer = resultSerializer;
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>An object.</returns>
        protected override AggregateResult<TDocument> DeserializeValue(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;

            var response = new BsonDocument();
            IEnumerable results = null;

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

            return new AggregateResult<TDocument>(response, results);
        }

        // private methods
        private IEnumerable ReadResults(BsonDeserializationContext context)
        {
            var bsonReader = context.Reader;
            var values = new List<object>();

            bsonReader.ReadStartArray();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                values.Add(context.DeserializeWithChildContext(_resultSerializer));
            }

            bsonReader.ReadEndArray();
            return values;
        }
    }
}