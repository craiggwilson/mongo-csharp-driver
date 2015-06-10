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
using System.Collections.Generic;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Events
{
    /// <preliminary/>
    /// <summary>
    /// Occurs after a message has been sent.
    /// </summary>
    public struct ConnectionSentMessagesEvent
    {
        private readonly ConnectionId _connectionId;
        private readonly TimeSpan _elapsedNetwork;
        private readonly TimeSpan _elapsedSerialization;
        private readonly int _length;
        private readonly IReadOnlyList<int> _requestIds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionSentMessagesEvent" /> struct.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="requestIds">The request ids.</param>
        /// <param name="length">The length.</param>
        /// <param name="elapsedNetwork">The elapsed network.</param>
        /// <param name="elapsedSerialization">The elapsed serialization.</param>
        public ConnectionSentMessagesEvent(ConnectionId connectionId, IReadOnlyList<int> requestIds, int length, TimeSpan elapsedNetwork, TimeSpan elapsedSerialization)
        {
            _connectionId = connectionId;
            _requestIds = requestIds;
            _length = length;
            _elapsedNetwork = elapsedNetwork;
            _elapsedSerialization = elapsedSerialization;
        }

        /// <summary>
        /// Gets the connection identifier.
        /// </summary>
        public ConnectionId ConnectionId
        {
            get { return _connectionId; }
        }

        /// <summary>
        /// Gets the elapsed.
        /// </summary>
        public TimeSpan Elapsed
        {
            get { return _elapsedNetwork + _elapsedSerialization; }
        }

        /// <summary>
        /// Gets the elapsed network.
        /// </summary>
        public TimeSpan ElapsedNetwork
        {
            get { return _elapsedNetwork; }
        }

        /// <summary>
        /// Gets the elapsed serialization.
        /// </summary>
        public TimeSpan ElapsedSerialization
        {
            get { return _elapsedSerialization; }
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        public int Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Gets the request ids.
        /// </summary>
        public IReadOnlyList<int> RequestIds
        {
            get { return _requestIds; }
        }
    }
}
