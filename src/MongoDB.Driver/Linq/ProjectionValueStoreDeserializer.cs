/* Copyright 2010-2012 10gen Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// A deserializer that utilizes a list of BsonSerializationInfos to dump information into an IProjectionValueStore.
    /// </summary>
    internal class ProjectionValueStoreDeserializer : SerializerBase<IProjectionValueStore>
    {
        // private fields
        private readonly Dictionary<string, BsonSerializationInfo> _deserializationMap;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionValueStoreDeserializer"/> class.
        /// </summary>
        /// <param name="deserializationInfo">The serialization info used for deserialization.</param>
        public ProjectionValueStoreDeserializer(IEnumerable<BsonSerializationInfo> deserializationInfo)
        {
            _deserializationMap = deserializationInfo.Distinct().ToDictionary(x => x.ElementName, x => x);
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>An object.</returns>
        public override IProjectionValueStore Deserialize(BsonDeserializationContext context)
        {
            var type = context.Reader.GetCurrentBsonType();
            var store = ReadDocument(context, null, null, new DocumentProjectionValueStore());
            return store;
        }

        // private methods
        private string BuildElementName(string prefix, string name)
        {
            if (prefix == null)
            {
                return name;
            }

            return prefix + "." + name;
        }

        private IProjectionValueStore ReadArray(BsonDeserializationContext context, string currentKey)
        {
            var bsonReader = context.Reader;
            bsonReader.ReadStartArray();
            var arrayStore = new ArrayProjectionValueStore();
            BsonType bsonType;
            while ((bsonType = bsonReader.ReadBsonType()) != BsonType.EndOfDocument)
            {
                var currentBsonType = bsonReader.GetCurrentBsonType();
                if (currentBsonType == BsonType.Document)
                {
                    arrayStore.AddValue(ReadDocument(context, currentKey, null, new DocumentProjectionValueStore()));
                }
                else if (currentBsonType == BsonType.Array)
                {
                    arrayStore.AddValue(ReadArray(context, currentKey));
                }
                else
                {
                    // we should never get here because we, presumably, have only pulled back things we know about...
                    throw LinqErrors.UnexpectedFieldInProjection(currentKey);
                }
            }
            bsonReader.ReadEndArray();
            return arrayStore;
        }

        private IProjectionValueStore ReadDocument(BsonDeserializationContext context, string currentKey, string scopeKey, DocumentProjectionValueStore documentStore)
        {
            var bsonReader = context.Reader;
            bsonReader.ReadStartDocument();
            BsonType bsonType;
            while ((bsonType = bsonReader.ReadBsonType()) != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();
                var newCurrentKey = BuildElementName(currentKey, name);
                var newScopeKey = BuildElementName(scopeKey, name);
                BsonSerializationInfo serializationInfo;
                if (_deserializationMap.TryGetValue(newCurrentKey, out serializationInfo))
                {
                    var value = context.DeserializeWithChildContext(serializationInfo.Serializer);
                    var singleValueStore = new SingleValueProjectionValueStore();
                    singleValueStore.SetValue(value);
                    documentStore.SetValue(newScopeKey, singleValueStore);
                }
                else
                {
                    var nestedBsonType = bsonReader.GetCurrentBsonType();
                    if (bsonType == BsonType.Document)
                    {
                        // we are going to read nested documents into the same documentStore to keep them flat, optimized for lookup
                        ReadDocument(context, newCurrentKey, newScopeKey, documentStore);
                    }
                    else if (bsonType == BsonType.Array)
                    {
                        documentStore.SetValue(name, ReadArray(context, newCurrentKey));
                    }
                    else
                    {
                        bsonReader.SkipValue();
                    }
                }
            }
            bsonReader.ReadEndDocument();
            return documentStore;
        }
    }
}