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
    /// <summary>
    /// Represents a serializer for a AggregateCommandResult with values of type TValue.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class AggregateCommandResultSerializer<TValue> : BsonBaseSerializer
    {
        // private fields
        private readonly IBsonSerializer _resultSerializer;
        private readonly IBsonSerializationOptions _resultSerializationOptions;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateCommandResultSerializer{TValue}"/> class.
        /// </summary>
        public AggregateCommandResultSerializer()
            : this(BsonSerializer.LookupSerializer(typeof(TValue)), null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateCommandResultSerializer{TValue}" /> class.
        /// </summary>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <param name="resultSerializationOptions">The result serialization options.</param>
        public AggregateCommandResultSerializer(IBsonSerializer resultSerializer, IBsonSerializationOptions resultSerializationOptions)
        {
            _resultSerializer = resultSerializer;
            _resultSerializationOptions = resultSerializationOptions;
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        {
            var response = new BsonDocument();
            IEnumerable<TValue> results = null;

            bsonReader.ReadStartDocument();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();
                if (name == "result")
                {
                    results = ReadResults(bsonReader);
                }
                else
                {
                    var value = (BsonValue)BsonValueSerializer.Instance.Deserialize(bsonReader, typeof(BsonValue), null);
                    response.Add(name, value);
                }
            }
            bsonReader.ReadEndDocument();

            return new AggregateCommandResult<TValue>(response, results);
        }

        // private methods
        private IEnumerable<TValue> ReadResults(BsonReader bsonReader)
        {
            var values = new List<TValue>();

            bsonReader.ReadStartArray();
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                values.Add((TValue)_resultSerializer.Deserialize(bsonReader, typeof(TValue), _resultSerializationOptions));
            }
            bsonReader.ReadEndArray();

            return values;
        }
    }
}