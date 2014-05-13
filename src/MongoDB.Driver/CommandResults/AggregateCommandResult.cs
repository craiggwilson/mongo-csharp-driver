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
    /// <summary>
    /// The result of an aggregate command.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    [Serializable]
    [BsonSerializer(typeof(AggregateCommandResultSerializer<>))]
    public class AggregateCommandResult<TResult> : CommandResult
    {
        // private fields
        private readonly IEnumerable<TResult> _results;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateCommandResult{TResult}" /> class.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="results">The values.</param>
        internal AggregateCommandResult(BsonDocument response, IEnumerable<TResult> results)
            : base(response)
        {
            _results = results;
        }

        // public properties
        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>
        /// The values.
        /// </value>
        public IEnumerable<TResult> Results
        {
            get { return _results; }
        }
    }
}
