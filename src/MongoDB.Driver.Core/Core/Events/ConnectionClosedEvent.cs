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

using System;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// Represents information about a ConnectionAfterClosing event.
    /// </summary>
    public struct ConnectionClosedEvent
    {
        private readonly ConnectionId _connectionId;
        private readonly TimeSpan _elapsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionClosedEvent" /> struct.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="elapsed">The elapsed.</param>
        public ConnectionClosedEvent(ConnectionId connectionId, TimeSpan elapsed)
        {
            _connectionId = connectionId;
            _elapsed = elapsed;
        }

        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        /// <value>
        /// The connection identifier.
        /// </value>
        public ConnectionId ConnectionId
        {
            get { return _connectionId; }
        }
    }
}
