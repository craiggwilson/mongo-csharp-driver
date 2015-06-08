using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Common.Logging;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Common.Logging
{
    internal class CommonLogWriter : IEventRegistrar
    {
        private readonly ILog _clusterLog = LogManager.GetLogger("MongoDB.Driver.Core.Cluster");
        private readonly ILog _serverLog = LogManager.GetLogger("MongoDB.Driver.Core.Server");
        private readonly ILog _connectionPoolLog = LogManager.GetLogger("MongoDB.Driver.Core.ConnectionPool");
        private readonly ILog _connectionLog = LogManager.GetLogger("MongoDB.Driver.Core.Connection");

        public void Register(IEventAggregator eventAggregator)
        {
            new ReflectionEventRegistrar(this).Register(eventAggregator);
        }

        // Clusters
        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterClosingEvent @event)
        {
            _clusterLog.DebugFormat("{0}: closing.", Label(@event.ClusterId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterClosedEvent @event)
        {
            _clusterLog.InfoFormat("{0}: closed in {1}ms.", Label(@event.ClusterId), @event.Elapsed.TotalMilliseconds.ToString());
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterOpeningEvent @event)
        {
            _clusterLog.DebugFormat("{0}: opening.", Label(@event.ClusterId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterOpenedEvent @event)
        {
            _clusterLog.InfoFormat("{0}: opened in {1}ms.", Label(@event.ClusterId), @event.Elapsed.TotalMilliseconds.ToString());
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterAddingServerEvent @event)
        {
            _clusterLog.DebugFormat("{0}: adding server at endpoint {1}.", Label(@event.ClusterId), Format(@event.EndPoint));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterAddedServerEvent @event)
        {
            _clusterLog.InfoFormat("{0}: added server {1} in {2}ms.", Label(@event.ServerId.ClusterId), Format(@event.ServerId), @event.Elapsed.TotalMilliseconds.ToString());
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterRemovingServerEvent @event)
        {
            _clusterLog.DebugFormat("{0}: removing server {1}. Reason: {2}", Label(@event.ServerId.ClusterId), Format(@event.ServerId), @event.Reason);
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterRemovedServerEvent @event)
        {
            _clusterLog.InfoFormat("{0}: removed server {1} in {2}ms. Reason: {3}", Label(@event.ServerId.ClusterId), Format(@event.ServerId), @event.Elapsed.TotalMilliseconds.ToString(), @event.Reason);
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ClusterDescriptionChangedEvent @event)
        {
            _clusterLog.InfoFormat("{0}: {1}", Label(@event.OldDescription.ClusterId), @event.NewDescription);
        }

        // Servers
        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerClosingEvent @event)
        {
            _serverLog.DebugFormat("{0}: closing.", Label(@event.ServerId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerClosedEvent @event)
        {
            _serverLog.InfoFormat("{0}: closed in {1}ms.", Label(@event.ServerId), @event.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerOpeningEvent @event)
        {
            _serverLog.DebugFormat("{0}: opening.", Label(@event.ServerId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerOpenedEvent @event)
        {
            _serverLog.InfoFormat("{0}: opened in {1}ms.", Label(@event.ServerId), @event.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerHeartbeatStartedEvent @event)
        {
            _serverLog.DebugFormat("{0}: sending heartbeat.", Label(@event.ConnectionId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerHeartbeatSucceededEvent @event)
        {
            _serverLog.InfoFormat("{0}: sent heartbeat in {1}ms.", Label(@event.ConnectionId), @event.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerHeartbeatFailedEvent @event)
        {
            _serverLog.ErrorFormat("{0}: error sending heartbeat.", @event.Exception, Label(@event.ConnectionId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ServerDescriptionChangedEvent @event)
        {
            _serverLog.InfoFormat("{0}: {1}", Label(@event.OldDescription.ServerId), @event.NewDescription);
        }

        // Connection Pools
        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolClosingEvent @event)
        {
            _connectionPoolLog.DebugFormat("{0}-pool: closing.", Label(@event.ServerId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolClosedEvent @event)
        {
            _connectionPoolLog.InfoFormat("{0}-pool: closed.", Label(@event.ServerId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolOpeningEvent @event)
        {
            _connectionPoolLog.DebugFormat("{0}-pool: opening.", Label(@event.ServerId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolOpenedEvent @event)
        {
            _connectionPoolLog.InfoFormat("{0}-pool: opened.", Label(@event.ServerId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolAddingConnectionEvent @event)
        {
            _connectionPoolLog.DebugFormat("{0}-pool: adding connection.", Label(@event.ServerId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolAddedConnectionEvent @event)
        {
            _connectionPoolLog.InfoFormat("{0}-pool: added connection {1} in {2}ms.", Label(@event.ConnectionId.ServerId), Format(@event.ConnectionId), @event.Elapsed.TotalMilliseconds.ToString());
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolRemovingConnectionEvent @event)
        {
            _connectionPoolLog.DebugFormat("{0}-pool: removing connection {1}.", Label(@event.ConnectionId.ServerId), Format(@event.ConnectionId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolRemovedConnectionEvent @event)
        {
            _connectionPoolLog.InfoFormat("{0}-pool: removed connection {1} in {2}ms.", Label(@event.ConnectionId.ServerId), Format(@event.ConnectionId), @event.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolCheckingOutConnectionEvent @event)
        {
            _connectionPoolLog.DebugFormat("{0}-pool: checking out a connection.", Label(@event.ServerId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolCheckedOutConnectionEvent @event)
        {
            _connectionPoolLog.InfoFormat("{0}-pool: checked out connection {1} in {2}ms.", Label(@event.ConnectionId.ServerId), Format(@event.ConnectionId), @event.Elapsed.TotalMilliseconds.ToString());
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolCheckingOutConnectionFailedEvent @event)
        {
            _connectionPoolLog.ErrorFormat("{0}-pool: error checking out a connection.", @event.Exception, Label(@event.ServerId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolCheckingInConnectionEvent @event)
        {
            _connectionPoolLog.DebugFormat("{0}-pool: checking in connection {1}.", Label(@event.ConnectionId.ServerId), Format(@event.ConnectionId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionPoolCheckedInConnectionEvent @event)
        {
            _connectionPoolLog.InfoFormat("{0}-pool: checked in connection {1} in {2}ms.", Label(@event.ConnectionId.ServerId), Format(@event.ConnectionId), @event.Elapsed.TotalMilliseconds.ToString());
        }

        // Connections
        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionFailedEvent @event)
        {
            _connectionLog.ErrorFormat("{0}: failed.", @event.Exception, Label(@event.ConnectionId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionClosingEvent @event)
        {
            _connectionLog.DebugFormat("{0}: closing.", Label(@event.ConnectionId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionClosedEvent @event)
        {
            _connectionLog.InfoFormat("{0}: closed.", Label(@event.ConnectionId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionOpeningEvent @event)
        {
            _connectionLog.DebugFormat("{0}: opening.", Label(@event.ConnectionId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionOpenedEvent @event)
        {
            _connectionLog.InfoFormat("{0}: opened in {1}ms.", Label(@event.ConnectionId), @event.Elapsed.TotalMilliseconds.ToString());
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionOpeningFailedEvent @event)
        {
            _connectionLog.ErrorFormat("{0}: unable to open.", @event.Exception, Label(@event.ConnectionId));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionReceivingMessageEvent @event)
        {
            _connectionLog.DebugFormat("{0}: receiving message in response to {1}.", Label(@event.ConnectionId), @event.ResponseTo.ToString());
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionReceivedMessageEvent @event)
        {
            _connectionLog.InfoFormat("{0}: received message in response to {1} of length {2} bytes in {3}ms.", Label(@event.ConnectionId), @event.ResponseTo.ToString(), @event.Length.ToString(), @event.Elapsed.TotalMilliseconds.ToString());
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionReceivingMessageFailedEvent @event)
        {
            _connectionLog.ErrorFormat("{0}: error receiving message in response to {1}.", @event.Exception, Label(@event.ConnectionId), @event.ResponseTo.ToString());
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionSendingMessagesEvent @event)
        {
            _connectionLog.DebugFormat("{0}: sending messages [{1}].", Label(@event.ConnectionId), string.Join(",", @event.RequestIds));
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionSentMessagesEvent @event)
        {
            _connectionLog.InfoFormat("{0}: sent messages [{1}] of length {2} bytes in {3}ms.", Label(@event.ConnectionId), string.Join(",", @event.RequestIds), @event.Length.ToString(), @event.Elapsed.TotalMilliseconds.ToString());
        }

        /// <summary>
        /// Handles the specified event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Handle(ConnectionSendingMessagesFailedEvent @event)
        {
            _connectionLog.ErrorFormat("{0}: error sending messages [{1}].", @event.Exception, Label(@event.ConnectionId), string.Join(",", @event.RequestIds));
        }

        private string Label(ConnectionId id)
        {
            var format = "{0}:{1}:{2}";
            string connId;
            if (id.ServerValue.HasValue)
            {
                connId = id.LocalValue.ToString() + "-" + id.ServerValue.Value.ToString();
            }
            else
            {
                connId = id.LocalValue.ToString();
            }

            return string.Format(format, id.ServerId.ClusterId.Value.ToString(), Format(id.ServerId.EndPoint), connId);
        }

        private string Label(ServerId serverId)
        {
            return string.Concat(
                serverId.ClusterId.Value.ToString(),
                ":",
                Format(serverId.EndPoint));
        }

        private string Label(ClusterId clusterId)
        {
            return clusterId.Value.ToString();
        }

        private string Format(ConnectionId id)
        {
            if (id.ServerValue.HasValue)
            {
                return id.LocalValue.ToString() + "-" + id.ServerValue.Value.ToString();
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
                    dnsEndPoint.Port.ToString());
            }

            return endPoint.ToString();
        }
    }
}
