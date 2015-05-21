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
using System.Diagnostics;
using System.Net;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Clusters
{
    /// <summary>
    /// Represents a standalone cluster.
    /// </summary>
    internal sealed class SingleServerCluster : Cluster
    {
        // fields
        private IClusterableServer _server;
        private readonly InterlockedInt32 _state;

        private readonly Action<ClusterBeforeClosingEvent> _beforeClosingEventHandler;
        private readonly Action<ClusterAfterClosingEvent> _afterClosingEventHandler;
        private readonly Action<ClusterBeforeOpeningEvent> _beforeOpeningEventHandler;
        private readonly Action<ClusterAfterOpeningEvent> _afterOpeningEventHandler;
        private readonly Action<ClusterBeforeAddingServerEvent> _beforeAddingServerEventHandler;
        private readonly Action<ClusterAfterAddingServerEvent> _afterAddingServerEventHandler;

        // constructor
        internal SingleServerCluster(ClusterSettings settings, IClusterableServerFactory serverFactory, IEventSubscriber eventSubscriber)
            : base(settings, serverFactory, eventSubscriber)
        {
            Ensure.IsEqualTo(settings.EndPoints.Count, 1, "settings.EndPoints.Count");

            _state = new InterlockedInt32(State.Initial);

            eventSubscriber.TryGetEventHandler(out _beforeClosingEventHandler);
            eventSubscriber.TryGetEventHandler(out _afterClosingEventHandler);
            eventSubscriber.TryGetEventHandler(out _beforeOpeningEventHandler);
            eventSubscriber.TryGetEventHandler(out _afterOpeningEventHandler);
            eventSubscriber.TryGetEventHandler(out _beforeAddingServerEventHandler);
            eventSubscriber.TryGetEventHandler(out _afterAddingServerEventHandler);
        }

        // methods
        protected override void Dispose(bool disposing)
        {
            if (_state.TryChange(State.Disposed))
            {
                if (disposing)
                {
                    if (_beforeClosingEventHandler != null)
                    {
                        _beforeClosingEventHandler(new ClusterBeforeClosingEvent(ClusterId));
                    }

                    var stopwatch = Stopwatch.StartNew();
                    if (_server != null)
                    {
                        _server.DescriptionChanged -= ServerDescriptionChanged;
                        _server.Dispose();
                    }
                    stopwatch.Stop();

                    if (_afterClosingEventHandler != null)
                    {
                        _afterClosingEventHandler(new ClusterAfterClosingEvent(ClusterId, stopwatch.Elapsed));
                    }
                }
            }
            base.Dispose(disposing);
        }

        public override void Initialize()
        {
            base.Initialize();
            if (_state.TryChange(State.Initial, State.Open))
            {
                if (_beforeOpeningEventHandler != null)
                {
                    _beforeOpeningEventHandler(new ClusterBeforeOpeningEvent(ClusterId, Settings));
                }
                if (_beforeAddingServerEventHandler != null)
                {
                    _beforeAddingServerEventHandler(new ClusterBeforeAddingServerEvent(ClusterId, Settings.EndPoints[0]));
                }

                var stopwatch = Stopwatch.StartNew();
                _server = CreateServer(Settings.EndPoints[0]);
                _server.DescriptionChanged += ServerDescriptionChanged;
                _server.Initialize();
                stopwatch.Stop();

                if (_afterAddingServerEventHandler != null)
                {
                    _afterAddingServerEventHandler(new ClusterAfterAddingServerEvent(_server.ServerId, stopwatch.Elapsed));
                }
                if (_afterOpeningEventHandler != null)
                {
                    _afterOpeningEventHandler(new ClusterAfterOpeningEvent(ClusterId, Settings, stopwatch.Elapsed));
                }
            }
        }

        private bool IsServerValidForCluster(ClusterType clusterType, ClusterConnectionMode connectionMode, ServerType serverType)
        {
            switch (clusterType)
            {
                case ClusterType.ReplicaSet:
                    return serverType.IsReplicaSetMember();

                case ClusterType.Sharded:
                    return serverType == ServerType.ShardRouter;

                case ClusterType.Standalone:
                    return serverType == ServerType.Standalone;

                case ClusterType.Unknown:
                    switch (connectionMode)
                    {
                        case ClusterConnectionMode.Automatic:
                        case ClusterConnectionMode.Direct:
                            return true;

                        default:
                            throw new MongoInternalException("Unexpected connection mode.");
                    }

                default:
                    throw new MongoInternalException("Unexpected cluster type.");
            }
        }

        protected override void RequestHeartbeat()
        {
            _server.RequestHeartbeat();
        }

        private void ServerDescriptionChanged(object sender, ServerDescriptionChangedEventArgs args)
        {
            var newServerDescription = args.NewServerDescription;
            var newClusterDescription = Description;

            if (newServerDescription.State == ServerState.Disconnected)
            {
                newClusterDescription = newClusterDescription.WithServerDescription(newServerDescription);
            }
            else
            {
                if (IsServerValidForCluster(newClusterDescription.Type, Settings.ConnectionMode, newServerDescription.Type))
                {
                    if (newClusterDescription.Type == ClusterType.Unknown)
                    {
                        newClusterDescription = newClusterDescription.WithType(newServerDescription.Type.ToClusterType());
                    }

                    newClusterDescription = newClusterDescription.WithServerDescription(newServerDescription);
                }
                else
                {
                    newClusterDescription = newClusterDescription.WithoutServerDescription(newServerDescription.EndPoint);
                }
            }

            UpdateClusterDescription(newClusterDescription);
        }

        protected override bool TryGetServer(EndPoint endPoint, out IClusterableServer server)
        {
            if (EndPointHelper.Equals(_server.EndPoint, endPoint))
            {
                server = _server;
                return true;
            }
            else
            {
                server = null;
                return false;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_state.Value == State.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        // nested classes
        private static class State
        {
            public const int Initial = 0;
            public const int Open = 1;
            public const int Disposed = 2;
        }
    }
}
