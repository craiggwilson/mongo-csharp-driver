﻿/* Copyright 2010-2014 MongoDB Inc.
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
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Operations;

namespace MongoDB.Driver
{
    using MongoDB.Bson.Serialization;

    internal class AggregateEnumerableResult<TDocument> : IEnumerable<TDocument>
    {
        // private fields
        private readonly MongoCollection _collection;
        private readonly AggregateArgs _args;
        private readonly IBsonSerializer _serializer;
        private readonly string _outputCollectionName;

        // constructors
        public AggregateEnumerableResult(
            MongoCollection collection,
            AggregateArgs args,
            IBsonSerializer serializer,
            string outputCollectionName)
        {
            _collection = collection;
            _args = args; // TODO: make a defensive copy?
            _serializer = serializer;
            _outputCollectionName = outputCollectionName;
        }

        // public methods
        public IEnumerator<TDocument> GetEnumerator()
        {
            if (_outputCollectionName != null)
            {
                var database = _collection.Database;
                var collectionSettings = new MongoCollectionSettings { ReadPreference = ReadPreference.Primary };
                var collection = database.GetCollection<TDocument>(_outputCollectionName, collectionSettings);

                return collection.FindAll().GetEnumerator();
            }

            var result = _collection.RunAggregateCommand<TDocument>(_args, _serializer);
            if (result.CursorId != 0)
            {
                var connectionProvider = new ServerInstanceConnectionProvider(result.ServerInstance);
                var readerSettings = new BsonBinaryReaderSettings
                {
                    Encoding = _collection.Settings.ReadEncoding ?? MongoDefaults.ReadEncoding,
                    GuidRepresentation = _collection.Settings.GuidRepresentation
                };

                return new CursorEnumerator<TDocument>(
                    connectionProvider,
                    _collection.FullName,
                    result.ResultDocuments,
                    result.CursorId,
                    _args.BatchSize ?? 0,
                    0,
                    readerSettings,
                    BsonSerializer.LookupSerializer<TDocument>());
            }
            else if (result.ResultDocuments != null)
            {
                return result.ResultDocuments.GetEnumerator();
            }
            else
            {
                throw new NotSupportedException("Unexpected response to aggregate command.");
            }
        }

        // explicit interface implementations
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
