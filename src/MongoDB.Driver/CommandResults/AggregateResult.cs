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
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDB.Driver
{
    using System.Collections;
    using System.Linq;

    /// <summary>
    /// The result of an aggregate command where the document type is known.
    /// </summary>
    /// <typeparam name="TDocument">
    /// </typeparam>
    public class AggregateResult<TDocument> : AggregateResult
    {
        internal AggregateResult(BsonDocument response, IEnumerable resultDocuments)
            : base(response, resultDocuments)
        {
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public new IEnumerable<TDocument> ResultDocuments
        {
            get { return base.ResultDocuments.Cast<TDocument>(); }
        }
    }

    /// <summary>
    /// The result of an aggregate command.
    /// </summary>
    [Serializable]
    [BsonSerializer(typeof(AggregateCommandResultSerializer<>))]
    public class AggregateResult : CommandResult
    {
        // private fields
        private readonly long _cursorId;

        private readonly IEnumerable _resultDocuments;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateResult" /> class.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="resultDocuments">The values.</param>
        internal AggregateResult(BsonDocument response, IEnumerable resultDocuments)
            : base(response)
        {
            if (response.Contains("cursor"))
            {
                var cursorDocument = response["cursor"];
                _cursorId = cursorDocument["id"].ToInt64();
            }

            this._resultDocuments = resultDocuments;
        }

        // public properties
        /// <summary>
        /// Gets the cursor id.
        /// </summary>
        /// <value>
        /// The cursor id.
        /// </value>
        public long CursorId
        {
            get { return _cursorId; }
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public IEnumerable ResultDocuments
        {
            get { return this._resultDocuments; }
        }
    }
}
