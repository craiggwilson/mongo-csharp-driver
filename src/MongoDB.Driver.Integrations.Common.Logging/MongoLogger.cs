/* Copyright 2010-2014 MongoDB Inc.
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
using System.Globalization;
using System.Net;
using Common.Logging;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Integrations.Common.Logging
{
    public class MongoLogger : IEventSubscriber
    {
        private static readonly ILog _clusterLog = LogManager.GetLogger("MongoDB.Driver.Core.Clusters");
        private static readonly ILog _serverLog = LogManager.GetLogger("MongoDB.Driver.Core.Servers");
        private static readonly ILog _connectionPoolLog = LogManager.GetLogger("MongoDB.Driver.Core.ConnectionPools");
        private static readonly ILog _connectionLog = LogManager.GetLogger("MongoDB.Driver.Core.Connections");
        private readonly IEventSubscriber _eventSubscriber;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoLogger"/> class.
        /// </summary>
        public MongoLogger()
        {
            _eventSubscriber = new ReflectionEventSubscriber(this);
        }

        /// <summary>
        /// Tries the get an event handler for an event of type <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="handler">The handler.</param>
        /// <returns><c>true</c> if this subscriber has provided an event handler; otherwise <c>false</c>.</returns>
        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
        {
            return _eventSubscriber.TryGetEventHandler(out handler);
        }

        // Clusters
        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterAddingServerEvent @event)
        {
            if (_clusterLog.IsDebugEnabled)
            {
                _clusterLog.DebugFormat(
                    "{0}: adding server at endpoint {1}.",
                    Label(@event.ClusterId),
                    Format(@event.EndPoint));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterAddedServerEvent @event)
        {
            if (_clusterLog.IsInfoEnabled)
            {
                _clusterLog.InfoFormat("{0}: added server {1} in {2}ms.",
                    Label(@event.ClusterId),
                    Format(@event.ServerId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterClosingEvent @event)
        {
            if (_clusterLog.IsDebugEnabled)
            {
                _clusterLog.DebugFormat("{0}: closing.",
                    Label(@event.ClusterId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterClosedEvent @event)
        {
            if (_clusterLog.IsInfoEnabled)
            {
                _clusterLog.InfoFormat("{0}: closed in {1}ms.",
                    Label(@event.ClusterId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterDescriptionChangedEvent @event)
        {
            if (_clusterLog.IsDebugEnabled)
            {
                _clusterLog.DebugFormat("{0}: {1}.",
                    Label(@event.ClusterId),
                    @event.NewDescription);
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterOpeningEvent @event)
        {
            if (_clusterLog.IsInfoEnabled)
            {
                _clusterLog.InfoFormat("{0}: opening.",
                    Label(@event.ClusterId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterOpenedEvent @event)
        {
            if (_clusterLog.IsDebugEnabled)
            {
                _clusterLog.DebugFormat("{0}: opened in {1}ms.",
                    Label(@event.ClusterId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterRemovingServerEvent @event)
        {
            if (_clusterLog.IsDebugEnabled)
            {
                _clusterLog.DebugFormat("{0}: removing server {1}. Reason: {2}.",
                    Label(@event.ClusterId),
                    Format(@event.ServerId),
                    @event.Reason);
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterRemovedServerEvent @event)
        {
            if (_clusterLog.IsInfoEnabled)
            {
                _clusterLog.InfoFormat("{0}: removed server {1} in {2}ms. Reason: {3}.",
                    Label(@event.ClusterId),
                    Format(@event.ServerId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                    @event.Reason);
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterSelectingServerEvent @event)
        {
            if (_clusterLog.IsDebugEnabled)
            {
                _clusterLog.DebugFormat("{0}: attempting to select a server with the selector {1}. Current cluster description: {2}.",
                    Label(@event.ClusterId),
                    @event.ServerSelector.ToString(),
                    @event.ClusterDescription.ToString());
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterSelectedServerEvent @event)
        {
            if (_clusterLog.IsDebugEnabled)
            {
                _clusterLog.DebugFormat("{0}: selected server {1} using the selector {2} in {3}ms. Cluster description: {4}.",
                    Label(@event.ClusterId),
                    @event.SelectedServer.ToString(),
                    @event.ServerSelector.ToString(),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                    @event.ClusterDescription.ToString());
            }
        }

        // Servers
        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerClosingEvent @event)
        {
            if (_serverLog.IsDebugEnabled)
            {
                _serverLog.DebugFormat("{0}: closing.",
                    Label(@event.ServerId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerClosedEvent @event)
        {
            if (_serverLog.IsInfoEnabled)
            {
                _serverLog.InfoFormat("{0}: closed in {1}ms.",
                    Label(@event.ServerId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }


        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerDescriptionChangedEvent @event)
        {
            if (_serverLog.IsDebugEnabled)
            {
                _serverLog.DebugFormat("{0}: {1}.",
                    Label(@event.ServerId),
                    @event.NewDescription.ToString());
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerHeartbeatStartedEvent @event)
        {
            if (_serverLog.IsDebugEnabled)
            {
                _serverLog.DebugFormat("{0}: sending heartbeat on connection {1}.",
                    Label(@event.ServerId),
                    Format(@event.ConnectionId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerHeartbeatSucceededEvent @event)
        {
            if (_serverLog.IsDebugEnabled)
            {
                _serverLog.DebugFormat("{0}: sent heartbeat in {1}ms on connection {2}.",
                    Label(@event.ServerId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                    Format(@event.ConnectionId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerHeartbeatFailedEvent @event)
        {
            if (_serverLog.IsWarnEnabled)
            {
                _serverLog.WarnFormat("{0}: error sending heartbeat on connection {1}.",
                    @event.Exception,
                    Label(@event.ServerId),
                    Format(@event.ConnectionId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerOpeningEvent @event)
        {
            if (_serverLog.IsInfoEnabled)
            {
                _serverLog.InfoFormat("{0}: opening.",
                    Label(@event.ServerId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerOpenedEvent @event)
        {
            if (_serverLog.IsDebugEnabled)
            {
                _serverLog.DebugFormat("{0}: opened in {1}ms.",
                    Label(@event.ServerId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        // Connection Pools
        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolAddingConnectionEvent @event)
        {
            if (_connectionPoolLog.IsDebugEnabled)
            {
                _connectionPoolLog.DebugFormat("{0}-pool: adding connection.",
                    Label(@event.ServerId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolAddedConnectionEvent @event)
        {
            if (_connectionPoolLog.IsInfoEnabled)
            {
                _connectionPoolLog.InfoFormat("{0}-pool: added connection {1} in {2}ms.",
                    Label(@event.ServerId),
                    Format(@event.ConnectionId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolCheckingInConnectionEvent @event)
        {
            if (_connectionPoolLog.IsDebugEnabled)
            {
                _connectionPoolLog.DebugFormat("{0}-pool: checking in connection {1}.",
                    Label(@event.ServerId),
                    Format(@event.ConnectionId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolCheckedInConnectionEvent @event)
        {
            if (_connectionPoolLog.IsDebugEnabled)
            {
                _connectionPoolLog.DebugFormat("{0}-pool: checked in connection {1} in {2}ms.",
                    Label(@event.ServerId),
                    Format(@event.ConnectionId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolCheckingOutConnectionEvent @event)
        {
            if (_connectionPoolLog.IsDebugEnabled)
            {
                _connectionPoolLog.DebugFormat("{0}-pool: checking out a connection.",
                    Label(@event.ServerId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolCheckedOutConnectionEvent @event)
        {
            if (_connectionPoolLog.IsDebugEnabled)
            {
                _connectionPoolLog.DebugFormat("{0}-pool: checked out connection {1} in {2}ms.",
                    Label(@event.ServerId),
                    Format(@event.ConnectionId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolCheckingOutConnectionFailedEvent @event)
        {
            if (_connectionPoolLog.IsErrorEnabled)
            {
                _connectionPoolLog.ErrorFormat("{0}-pool: error checking out a connection.",
                    @event.Exception,
                    Label(@event.ServerId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolClosingEvent @event)
        {
            if (_connectionPoolLog.IsDebugEnabled)
            {
                _connectionPoolLog.DebugFormat("{0}-pool: closing.",
                    Label(@event.ServerId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolClosedEvent @event)
        {
            if (_connectionPoolLog.IsInfoEnabled)
            {
                _connectionPoolLog.InfoFormat("{0}-pool: closed.",
                    Label(@event.ServerId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolOpeningEvent @event)
        {
            if (_connectionPoolLog.IsInfoEnabled)
            {
                _connectionPoolLog.InfoFormat("{0}-pool: opening.",
                    Label(@event.ServerId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolOpenedEvent @event)
        {
            if (_connectionPoolLog.IsDebugEnabled)
            {
                _connectionPoolLog.DebugFormat("{0}-pool: opened.",
                    Label(@event.ServerId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolRemovingConnectionEvent @event)
        {
            if (_connectionPoolLog.IsDebugEnabled)
            {
                _connectionPoolLog.DebugFormat("{0}-pool: removing connection {1}.",
                    Label(@event.ServerId),
                    Format(@event.ConnectionId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolRemovedConnectionEvent @event)
        {
            if (_connectionPoolLog.IsInfoEnabled)
            {
                _connectionPoolLog.InfoFormat("{0}-pool: removed connection {1} in {2}ms.",
                    Label(@event.ServerId),
                    Format(@event.ConnectionId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        // Connections
        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionClosingEvent @event)
        {
            if (_connectionLog.IsDebugEnabled)
            {
                _connectionLog.DebugFormat("{0}: closing.",
                    Label(@event.ConnectionId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionClosedEvent @event)
        {
            if (_connectionLog.IsInfoEnabled)
            {
                _connectionLog.InfoFormat("{0}: closed.",
                    Label(@event.ConnectionId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionFailedEvent @event)
        {
            if (_connectionLog.IsErrorEnabled)
            {
                _connectionLog.ErrorFormat("{0}: failed.",
                    @event.Exception,
                    Label(@event.ConnectionId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionOpeningEvent @event)
        {
            if (_connectionLog.IsInfoEnabled)
            {
                _connectionLog.InfoFormat("{0}: opening.",
                    Label(@event.ConnectionId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionOpenedEvent @event)
        {
            if (_connectionLog.IsDebugEnabled)
            {
                _connectionLog.DebugFormat("{0}: opened in {1}ms.",
                    Label(@event.ConnectionId),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionOpeningFailedEvent @event)
        {
            if (_connectionLog.IsErrorEnabled)
            {
                _connectionLog.ErrorFormat("{0}: error opening connection.",
                    @event.Exception,
                    Label(@event.ConnectionId));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionReceivingMessageEvent @event)
        {
            if (_connectionLog.IsDebugEnabled)
            {
                _connectionLog.DebugFormat("{0}: receiving message in response to {1}.",
                    Label(@event.ConnectionId),
                    @event.ResponseTo.ToString());
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionReceivedMessageEvent @event)
        {
            if (_connectionLog.IsDebugEnabled)
            {
                _connectionLog.InfoFormat("{0}: received message in response to {1} of length {2} bytes in {3}ms (network: {4}ms, deserialization: {5}ms).",
                    Label(@event.ConnectionId),
                    @event.ResponseTo.ToString(),
                    @event.Length.ToString(),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                    @event.NetworkDuration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                    @event.DeserializationDuration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionReceivingMessageFailedEvent @event)
        {
            if (_connectionLog.IsErrorEnabled)
            {
                _connectionLog.ErrorFormat("{0}: error receiving message in response to {1}.",
                    @event.Exception,
                    Label(@event.ConnectionId),
                    @event.ResponseTo.ToString());
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionSendingMessagesEvent @event)
        {
            if (_connectionLog.IsDebugEnabled)
            {
                _connectionLog.DebugFormat("{0}: sending messages [{1}].",
                    Label(@event.ConnectionId),
                    string.Join(",", @event.RequestIds));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionSentMessagesEvent @event)
        {
            if (_connectionLog.IsDebugEnabled)
            {
                _connectionLog.DebugFormat("{0}: sent messages [{1}] of length {2} bytes in {3}ms (network: {4}ms, serialization: {5}ms).",
                    Label(@event.ConnectionId),
                    string.Join(",", @event.RequestIds),
                    @event.Length.ToString(),
                    @event.Duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                    @event.NetworkDuration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture),
                    @event.SerializationDuration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionSendingMessagesFailedEvent @event)
        {
            if (_connectionLog.IsErrorEnabled)
            {
                _connectionLog.ErrorFormat("{0}: error sending messages [{1}].",
                    @event.Exception,
                    Label(@event.ConnectionId),
                    string.Join(",", @event.RequestIds));
            }
        }

        private string Label(ConnectionId id)
        {
            var format = "{0}:{1}:{2}";

            return string.Format(
                format,
                id.ServerId.ClusterId.Value.ToString(CultureInfo.InvariantCulture),
                Format(id.ServerId.EndPoint),
                Format(id));
        }

        private string Label(ServerId serverId)
        {
            return string.Concat(
                serverId.ClusterId.Value.ToString(CultureInfo.InvariantCulture),
                ":",
                Format(serverId.EndPoint));
        }

        private string Label(ClusterId clusterId)
        {
            return clusterId.Value.ToString(CultureInfo.InvariantCulture);
        }

        private string Format(ConnectionId id)
        {
            if (id.ServerValue.HasValue)
            {
                return id.LocalValue.ToString(CultureInfo.InvariantCulture) + "-" + id.ServerValue.Value.ToString(CultureInfo.InvariantCulture);
            }
            return id.LocalValue.ToString();
        }

        private string Format(ServerId serverId)
        {
            return Format(serverId.EndPoint);
        }

        private string Format(EndPoint endPoint)
        {
            var dnsEndPoint = endPoint as DnsEndPoint;
            if (dnsEndPoint != null)
            {
                return string.Concat(
                    dnsEndPoint.Host,
                    ":",
                    dnsEndPoint.Port.ToString(CultureInfo.InvariantCulture));
            }

            return endPoint.ToString();
        }
    }
}
