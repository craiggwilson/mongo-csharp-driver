/* Copyright 2017 MongoDB Inc.
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
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// A core session.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.Bindings.ICoreSession" />
    public sealed class CoreSession : ICoreSession
    {
        // private fields
        private readonly IClusterClock _clusterClock;
        private readonly BsonDocument _id;
        private readonly bool _isImplicitSession;
        private readonly IOperationClock _operationClock;
        private readonly Action _wasUsed;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSession" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="clusterClock">The cluster clock.</param>
        /// <param name="operationClock">The operation clock.</param>
        /// <param name="wasUsed">The was used.</param>
        /// <param name="isImplicitSession">if set to <c>true</c> the session is an implicit session.</param>
        public CoreSession(BsonDocument id, IClusterClock clusterClock, IOperationClock operationClock, Action wasUsed, bool isImplicitSession)
        {
            _id = id; // can be null
            _clusterClock = Ensure.IsNotNull(clusterClock, nameof(clusterClock));
            _operationClock = Ensure.IsNotNull(operationClock, nameof(operationClock));
            _wasUsed = Ensure.IsNotNull(wasUsed, nameof(wasUsed));
            _isImplicitSession = isImplicitSession;
        }

        // public properties
        /// <inheritdoc />
        public IClusterClock ClusterClock => _clusterClock;

        /// <inheritdoc />
        public BsonDocument Id => _id;

        /// <inheritdoc />
        public bool IsImplicitSession => _isImplicitSession;

        /// <inheritdoc />
        public IOperationClock OperationClock => _operationClock;

        // public methods
        /// <inheritdoc />
        public void WasUsed()
        {
            _wasUsed();
        }
    }
}
