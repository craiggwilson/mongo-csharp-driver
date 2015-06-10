/* Copyright 2013-2014 MongoDB Inc.
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

using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Clusters.ServerSelectors;

namespace MongoDB.Driver.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// Occurs before a server is selected.
    /// </summary>
    public struct ClusterSelectingServerEvent
    {
        private readonly ClusterId _clusterId;
        private readonly ClusterDescription _clusterDescription;
        private readonly IServerSelector _serverSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterSelectingServerEvent" /> struct.
        /// </summary>
        /// <param name="clusterId">The cluster identifier.</param>
        /// <param name="clusterDescription">The cluster description.</param>
        /// <param name="serverSelector">The server selector.</param>
        public ClusterSelectingServerEvent(ClusterId clusterId, ClusterDescription clusterDescription, IServerSelector serverSelector)
        {
            _clusterId = clusterId;
            _clusterDescription = clusterDescription;
            _serverSelector = serverSelector;
        }

        /// <summary>
        /// Gets the cluster identifier.
        /// </summary>
        public ClusterId ClusterId
        {
            get { return _clusterId; }
        }

        /// <summary>
        /// Gets the cluster description.
        /// </summary>
        public ClusterDescription ClusterDescription
        {
            get { return _clusterDescription; }
        }

        /// <summary>
        /// Gets the server selector.
        /// </summary>
        public IServerSelector ServerSelector
        {
            get { return _serverSelector; }
        }
    }
}
