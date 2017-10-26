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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Operations;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// The interface for a session in Core.
    /// </summary>
    public interface ICoreSession
    {
        // properties
        /// <summary>
        /// Gets the cluster clock.
        /// </summary>
        /// <value>
        /// The cluster clock.
        /// </value>
        IClusterClock ClusterClock { get; }

        /// <summary>
        /// Gets the session Id.
        /// </summary>
        /// <value>
        /// The session Id.
        /// </value>
        BsonDocument Id { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is implicit session.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is implicit session; otherwise, <c>false</c>.
        /// </value>
        bool IsImplicitSession { get; }

        /// <summary>
        /// Gets the operation clock.
        /// </summary>
        /// <value>
        /// The operation clock.
        /// </value>
        IOperationClock OperationClock { get; }

        // methods
        /// <summary>
        /// Called by the driver when the session is used (i.e. sent to the server).
        /// </summary>
        void WasUsed();
    }
}
