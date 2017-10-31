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
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// A server session pool.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.IServerSessionPool" />
    public class ServerSessionPool : IServerSessionPool
    {
        // private fields
        private readonly IMongoClient _client;
        private readonly object _lock = new object();
        private readonly List<IServerSession> _pool = new List<IServerSession>();

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerSessionPool" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public ServerSessionPool(IMongoClient client)
        {
            _client = Ensure.IsNotNull(client, nameof(client));
        }

        /// <inheritdoc />
        public IServerSession AcquireSession()
        {
            lock (_lock)
            {
                for (var i = _pool.Count - 1; i >= 0; i--)
                {
                    var serverSession = _pool[i];
                    if (!IsAboutToExpire(serverSession))
                    {
                        var removeCount = _pool.Count - i; // the one we're about to return and any about to expire ones we skipped over
                        _pool.RemoveRange(i, removeCount);
                        return new ReleaseOnDisposeServerSession(this, serverSession);
                    }
                }

                _pool.Clear(); // they're all about to expire
            }

            return new ReleaseOnDisposeServerSession(this, new ServerSession());
        }

        /// <inheritdoc />
        public void ReleaseSession(IServerSession serverSession)
        {
            lock (_lock)
            {
                var aboutToExpireCount = 0;
                for (var i = 0; i < _pool.Count; i++)
                {
                    if (IsAboutToExpire(_pool[i]))
                    {
                        aboutToExpireCount++;
                    }
                    else
                    {
                        break;
                    }
                }
                _pool.RemoveRange(0, aboutToExpireCount);

                if (!IsAboutToExpire(serverSession))
                {
                    _pool.Add(serverSession);
                }
            }
        }

        // private methods
        private bool IsAboutToExpire(IServerSession serverSession)
        {
            var logicalSessionTimeout = _client.Cluster.Description.LogicalSessionTimeout;
            if (!serverSession.LastUsedAt.HasValue || !logicalSessionTimeout.HasValue)
            {
                return true;
            }
            else
            {
                var expiresAt = serverSession.LastUsedAt.Value + logicalSessionTimeout.Value;
                var timeRemaining = expiresAt - DateTime.UtcNow;
                return timeRemaining < TimeSpan.FromMinutes(1);
            }
        }

        // nested types
        private sealed class ReleaseOnDisposeServerSession : IServerSession
        {
            // private fields
            private bool _disposed;
            private readonly IServerSessionPool _pool;
            private readonly IServerSession _serverSession;

            // constructors
            /// <summary>
            /// Initializes a new instance of the <see cref="ReleaseOnDisposeServerSession"/> class.
            /// </summary>
            /// <param name="pool">The pool.</param>
            /// <param name="serverSession">The server session.</param>
            public ReleaseOnDisposeServerSession(IServerSessionPool pool, IServerSession serverSession)
            {
                _pool = Ensure.IsNotNull(pool, nameof(pool));
                _serverSession = Ensure.IsNotNull(serverSession, nameof(serverSession));
            }

            // public properties
            /// <inheritdoc />
            public BsonDocument Id => _serverSession.Id;

            /// <inheritdoc />
            public DateTime? LastUsedAt => _serverSession.LastUsedAt;

            /// <inheritdoc />
            public void WasUsed()
            {
                _serverSession.WasUsed();
            }

            // public methods
            /// <inheritdoc />
            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    _pool.ReleaseSession(_serverSession);
                }
            }
        }
    }
}
