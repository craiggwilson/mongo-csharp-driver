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
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.WireProtocol.Messages;

namespace MongoDB.Driver.Core.Events
{
    /// <summary>
    /// Represents information about a ConnectionAfterReceivingMessage event.
    /// </summary>
    /// <preliminary />
    public struct ConnectionReceivedMessageEvent
    {
        private readonly ConnectionId _connectionId;
        private readonly TimeSpan _elapsedDeserialization;
        private readonly TimeSpan _elapsedNetwork;
        private readonly int _length;
        private readonly int _responseTo;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionReceivedMessageEvent" /> struct.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="responseTo">The response to.</param>
        /// <param name="length">The length.</param>
        /// <param name="elapsedNetwork">The elapsed network.</param>
        /// <param name="elapsedDeserialization">The elapsed deserialization.</param>
        public ConnectionReceivedMessageEvent(ConnectionId connectionId, int responseTo, int length, TimeSpan elapsedNetwork, TimeSpan elapsedDeserialization)
        {
            _connectionId = connectionId;
            _responseTo = responseTo;
            _length = length;
            _elapsedNetwork = elapsedNetwork;
            _elapsedDeserialization = elapsedDeserialization;
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

        /// <summary>
        /// Gets the elapsed.
        /// </summary>
        public TimeSpan Elapsed
        {
            get { return _elapsedNetwork + _elapsedDeserialization; }
        }

        /// <summary>
        /// Gets the elapsed deserialization.
        /// </summary>
        public TimeSpan ElapsedDeserialization
        {
            get { return _elapsedDeserialization; }
        }

        /// <summary>
        /// Gets the elapsed network.
        /// </summary>
        public TimeSpan ElapsedNetwork
        {
            get { return _elapsedNetwork; }
        }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length
        {
            get { return _length; }
        }

        /// <summary>
        /// Gets the received message.
        /// </summary>
        /// <value>
        /// The received message.
        /// </value>
        public int ResponseTo
        {
            get { return _responseTo; }
        }
    }
}
