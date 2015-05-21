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
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.ConnectionPools
{
    internal sealed class ExclusiveConnectionPool : IConnectionPool
    {
        // fields
        private readonly IConnectionFactory _connectionFactory;
        private readonly ListConnectionHolder _connectionHolder;
        private readonly EndPoint _endPoint;
        private int _generation;
        private readonly CancellationTokenSource _maintenanceCancellationTokenSource;
        private readonly WaitQueue _poolQueue;
        private readonly ServerId _serverId;
        private readonly ConnectionPoolSettings _settings;
        private readonly InterlockedInt32 _state;
        private readonly SemaphoreSlim _waitQueue;

        private readonly Action<ConnectionPoolBeforeEnteringWaitQueueEvent> _beforeEnteringWaitQueueEventHandler;
        private readonly Action<ConnectionPoolAfterEnteringWaitQueueEvent> _afterEnteringWaitQueueEventHandler;
        private readonly Action<ConnectionPoolErrorEnteringWaitQueueEvent> _errorEnteringWaitQueueEventHandler;
        private readonly Action<ConnectionPoolBeforeCheckingOutAConnectionEvent> _beforeCheckingOutAConnectionEventHandler;
        private readonly Action<ConnectionPoolAfterCheckingOutAConnectionEvent> _afterCheckingOutAConnectionEventHandler;
        private readonly Action<ConnectionPoolErrorCheckingOutAConnectionEvent> _errorCheckingOutAConnectionEventHandler;
        private readonly Action<ConnectionPoolBeforeCheckingInAConnectionEvent> _beforeCheckingInAConnectionEventHandler;
        private readonly Action<ConnectionPoolAfterCheckingInAConnectionEvent> _afterCheckingInAConnectionEventHandler;
        private readonly Action<ConnectionPoolBeforeAddingAConnectionEvent> _beforeAddingAConnectionEventHandler;
        private readonly Action<ConnectionPoolAfterAddingAConnectionEvent> _afterAddingAConnectionEventHandler;
        private readonly Action<ConnectionPoolBeforeOpeningEvent> _beforeOpeningEventHandler;
        private readonly Action<ConnectionPoolAfterOpeningEvent> _afterOpeningEventHandler;
        private readonly Action<ConnectionPoolBeforeClosingEvent> _beforeClosingEventHandler;
        private readonly Action<ConnectionPoolAfterClosingEvent> _afterClosingEventHandler;

        // constructors
        public ExclusiveConnectionPool(
            ServerId serverId,
            EndPoint endPoint,
            ConnectionPoolSettings settings,
            IConnectionFactory connectionFactory,
            IEventSubscriber eventSubscriber)
        {
            _serverId = Ensure.IsNotNull(serverId, "serverId");
            _endPoint = Ensure.IsNotNull(endPoint, "endPoint");
            _settings = Ensure.IsNotNull(settings, "settings");
            _connectionFactory = Ensure.IsNotNull(connectionFactory, "connectionFactory");
            Ensure.IsNotNull(eventSubscriber, "eventSubscriber");

            _connectionHolder = new ListConnectionHolder(eventSubscriber);
            _poolQueue = new WaitQueue(settings.MaxConnections);
            _waitQueue = new SemaphoreSlim(settings.WaitQueueSize);
            _maintenanceCancellationTokenSource = new CancellationTokenSource();
            _state = new InterlockedInt32(State.Initial);

            eventSubscriber.TryGetEventHandler(out _beforeEnteringWaitQueueEventHandler);
            eventSubscriber.TryGetEventHandler(out _afterEnteringWaitQueueEventHandler);
            eventSubscriber.TryGetEventHandler(out _errorEnteringWaitQueueEventHandler);
            eventSubscriber.TryGetEventHandler(out _beforeCheckingOutAConnectionEventHandler);
            eventSubscriber.TryGetEventHandler(out _afterCheckingOutAConnectionEventHandler);
            eventSubscriber.TryGetEventHandler(out _errorCheckingOutAConnectionEventHandler);
            eventSubscriber.TryGetEventHandler(out _beforeCheckingInAConnectionEventHandler);
            eventSubscriber.TryGetEventHandler(out _afterCheckingInAConnectionEventHandler);
            eventSubscriber.TryGetEventHandler(out _beforeAddingAConnectionEventHandler);
            eventSubscriber.TryGetEventHandler(out _afterAddingAConnectionEventHandler);
            eventSubscriber.TryGetEventHandler(out _beforeOpeningEventHandler);
            eventSubscriber.TryGetEventHandler(out _afterOpeningEventHandler);
            eventSubscriber.TryGetEventHandler(out _beforeClosingEventHandler);
            eventSubscriber.TryGetEventHandler(out _afterClosingEventHandler);
            eventSubscriber.TryGetEventHandler(out _beforeAddingAConnectionEventHandler);
            eventSubscriber.TryGetEventHandler(out _afterAddingAConnectionEventHandler);
        }

        // properties
        public int AvailableCount
        {
            get
            {
                ThrowIfDisposed();
                return _poolQueue.CurrentCount;
            }
        }

        public int CreatedCount
        {
            get
            {
                ThrowIfDisposed();
                return UsedCount + DormantCount;
            }
        }

        public int DormantCount
        {
            get
            {
                ThrowIfDisposed();
                return _connectionHolder.Count;
            }
        }

        public int Generation
        {
            get { return Interlocked.CompareExchange(ref _generation, 0, 0); }
        }

        public ServerId ServerId
        {
            get { return _serverId; }
        }

        public int UsedCount
        {
            get
            {
                ThrowIfDisposed();
                return _settings.MaxConnections - AvailableCount;
            }
        }

        // public methods
        public async Task<IConnectionHandle> AcquireConnectionAsync(CancellationToken cancellationToken)
        {
            ThrowIfNotOpen();

            bool enteredWaitQueue = false;
            bool enteredPool = false;

            var stopwatch = new Stopwatch();
            try
            {
                if (_beforeEnteringWaitQueueEventHandler != null)
                {
                    _beforeEnteringWaitQueueEventHandler(new ConnectionPoolBeforeEnteringWaitQueueEvent(_serverId));
                }

                stopwatch.Start();
                enteredWaitQueue = _waitQueue.Wait(0); // don't wait...
                if (!enteredWaitQueue)
                {
                    throw MongoWaitQueueFullException.ForConnectionPool(_endPoint);
                }
                stopwatch.Stop();

                if (_afterEnteringWaitQueueEventHandler != null)
                {
                    _afterEnteringWaitQueueEventHandler(new ConnectionPoolAfterEnteringWaitQueueEvent(_serverId, stopwatch.Elapsed));
                }
                if (_beforeCheckingOutAConnectionEventHandler != null)
                {
                    _beforeCheckingOutAConnectionEventHandler(new ConnectionPoolBeforeCheckingOutAConnectionEvent(_serverId));
                }

                stopwatch.Restart();
                enteredPool = await _poolQueue.WaitAsync(_settings.WaitQueueTimeout, cancellationToken).ConfigureAwait(false);

                if (enteredPool)
                {
                    var acquired = AcquireConnection();
                    stopwatch.Stop();
                    if (_afterCheckingOutAConnectionEventHandler != null)
                    {
                        _afterCheckingOutAConnectionEventHandler(new ConnectionPoolAfterCheckingOutAConnectionEvent(acquired.ConnectionId, stopwatch.Elapsed));
                    }
                    return acquired;
                }

                stopwatch.Stop();
                var message = string.Format("Timed out waiting for a connection after {0}ms.", stopwatch.ElapsedMilliseconds);
                throw new TimeoutException(message);
            }
            catch (Exception ex)
            {
                if (enteredPool)
                {
                    try
                    {
                        _poolQueue.Release();
                    }
                    catch
                    {
                        // TODO: log this, but don't throw... it's a bug if we get here
                    }
                }

                if (!enteredWaitQueue)
                {
                    if (_errorEnteringWaitQueueEventHandler != null)
                    {
                        _errorEnteringWaitQueueEventHandler(new ConnectionPoolErrorEnteringWaitQueueEvent(_serverId, ex));
                    }
                }
                else
                {
                    if (_errorCheckingOutAConnectionEventHandler != null)
                    {
                        _errorCheckingOutAConnectionEventHandler(new ConnectionPoolErrorCheckingOutAConnectionEvent(_serverId, ex));
                    }
                }
                throw;
            }
            finally
            {
                if (enteredWaitQueue)
                {
                    try
                    {
                        _waitQueue.Release();
                    }
                    catch
                    {
                        // TODO: log this, but don't throw... it's a bug if we get here
                    }
                }
            }
        }

        private IConnectionHandle AcquireConnection()
        {
            PooledConnection connection = _connectionHolder.Acquire();
            if (connection == null)
            {
                if (_beforeAddingAConnectionEventHandler != null)
                {
                    _beforeAddingAConnectionEventHandler(new ConnectionPoolBeforeAddingAConnectionEvent(_serverId));
                }
                var stopwatch = Stopwatch.StartNew();
                connection = CreateNewConnection();
                stopwatch.Stop();

                if (_afterAddingAConnectionEventHandler != null)
                {
                    _afterAddingAConnectionEventHandler(new ConnectionPoolAfterAddingAConnectionEvent(connection.ConnectionId, stopwatch.Elapsed));
                }
            }

            var reference = new ReferenceCounted<PooledConnection>(connection, x => ReleaseConnection(x));
            return new AcquiredConnection(this, reference);
        }

        public void Clear()
        {
            ThrowIfNotOpen();
            Interlocked.Increment(ref _generation);
        }

        private PooledConnection CreateNewConnection()
        {
            var connection = _connectionFactory.CreateConnection(_serverId, _endPoint);
            return new PooledConnection(this, connection);
        }

        public void Initialize()
        {
            ThrowIfDisposed();
            if (_state.TryChange(State.Initial, State.Open))
            {
                if (_beforeOpeningEventHandler != null)
                {
                    _beforeOpeningEventHandler(new ConnectionPoolBeforeOpeningEvent(_serverId, _settings));
                }

                MaintainSize().ConfigureAwait(false);

                if (_afterOpeningEventHandler != null)
                {
                    _afterOpeningEventHandler(new ConnectionPoolAfterOpeningEvent(_serverId, _settings));
                }
            }
        }

        public void Dispose()
        {
            if (_state.TryChange(State.Disposed))
            {
                if (_beforeClosingEventHandler != null)
                {
                    _beforeClosingEventHandler(new ConnectionPoolBeforeClosingEvent(_serverId));
                }

                _connectionHolder.Clear();
                _maintenanceCancellationTokenSource.Cancel();
                _maintenanceCancellationTokenSource.Dispose();
                _poolQueue.Dispose();
                _waitQueue.Dispose();
                if (_afterClosingEventHandler != null)
                {
                    _afterClosingEventHandler(new ConnectionPoolAfterClosingEvent(_serverId));
                }
            }
        }

        private async Task MaintainSize()
        {
            var maintenanceCancellationToken = _maintenanceCancellationTokenSource.Token;
            while (!maintenanceCancellationToken.IsCancellationRequested)
            {
                try
                {
                    await PrunePoolAsync(maintenanceCancellationToken).ConfigureAwait(false);
                    await EnsureMinSizeAsync(maintenanceCancellationToken).ConfigureAwait(false);
                    await Task.Delay(_settings.MaintenanceInterval, maintenanceCancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    // do nothing, this is called in the background and, quite frankly, should never
                    // result in an error
                }
            }
        }

        private async Task PrunePoolAsync(CancellationToken cancellationToken)
        {
            bool enteredPool = false;
            try
            {
                // if it takes too long to enter the pool, then the pool is fully utilized
                // and we don't want to mess with it.
                enteredPool = await _poolQueue.WaitAsync(TimeSpan.FromMilliseconds(20), cancellationToken).ConfigureAwait(false);
                if (!enteredPool)
                {
                    return;
                }

                _connectionHolder.Prune();
            }
            finally
            {
                if (enteredPool)
                {
                    try
                    {
                        _poolQueue.Release();
                    }
                    catch
                    {
                        // log this... it's a bug
                    }
                }
            }
        }

        private async Task EnsureMinSizeAsync(CancellationToken cancellationToken)
        {
            while (CreatedCount < _settings.MinConnections)
            {
                bool enteredPool = false;
                try
                {
                    enteredPool = await _poolQueue.WaitAsync(TimeSpan.FromMilliseconds(20), cancellationToken).ConfigureAwait(false);
                    if (!enteredPool)
                    {
                        return;
                    }

                    if (_beforeAddingAConnectionEventHandler != null)
                    {
                        _beforeAddingAConnectionEventHandler(new ConnectionPoolBeforeAddingAConnectionEvent(_serverId));
                    }

                    var stopwatch = Stopwatch.StartNew();
                    var connection = CreateNewConnection();
                    // when adding in a connection, we need to open it because 
                    // the whole point of having a min pool size is to have
                    // them available and ready...
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                    _connectionHolder.Return(connection);
                    stopwatch.Stop();

                    if (_afterAddingAConnectionEventHandler != null)
                    {
                        _afterAddingAConnectionEventHandler(new ConnectionPoolAfterAddingAConnectionEvent(connection.ConnectionId, stopwatch.Elapsed));
                    }
                }
                finally
                {
                    if (enteredPool)
                    {
                        try
                        {
                            _poolQueue.Release();
                        }
                        catch
                        {
                            // log this... it's a bug
                        }
                    }
                }
            }
        }

        private void ReleaseConnection(PooledConnection connection)
        {
            if (_state.Value == State.Disposed)
            {
                connection.Dispose();
                return;
            }

            if (_beforeCheckingInAConnectionEventHandler != null)
            {
                _beforeCheckingInAConnectionEventHandler(new ConnectionPoolBeforeCheckingInAConnectionEvent(connection.ConnectionId));
            }

            var stopwatch = Stopwatch.StartNew();
            _connectionHolder.Return(connection);
            _poolQueue.Release();
            stopwatch.Stop();

            if (_afterCheckingInAConnectionEventHandler != null)
            {
                _afterCheckingInAConnectionEventHandler(new ConnectionPoolAfterCheckingInAConnectionEvent(connection.ConnectionId, stopwatch.Elapsed));
            }
        }

        private void ThrowIfDisposed()
        {
            if (_state.Value == State.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private void ThrowIfNotOpen()
        {
            if (_state.Value != State.Open)
            {
                ThrowIfDisposed();
                throw new InvalidOperationException("ConnectionPool must be initialized.");
            }
        }

        // nested classes
        private static class State
        {
            public const int Initial = 0;
            public const int Open = 1;
            public const int Disposed = 2;
        }

        private sealed class PooledConnection : IConnection
        {
            private readonly IConnection _connection;
            private readonly ExclusiveConnectionPool _connectionPool;
            private readonly int _generation;

            public PooledConnection(ExclusiveConnectionPool connectionPool, IConnection connection)
            {
                _connectionPool = connectionPool;
                _connection = connection;
                _generation = connectionPool._generation;
            }

            public ConnectionId ConnectionId
            {
                get { return _connection.ConnectionId; }
            }

            public ConnectionDescription Description
            {
                get { return _connection.Description; }
            }

            public EndPoint EndPoint
            {
                get { return _connection.EndPoint; }
            }

            public bool IsExpired
            {
                get { return _generation < _connectionPool.Generation || _connection.IsExpired; }
            }

            public ConnectionSettings Settings
            {
                get { return _connection.Settings; }
            }

            public void Dispose()
            {
                _connection.Dispose();
            }

            public Task OpenAsync(CancellationToken cancellationToken)
            {
                return _connection.OpenAsync(cancellationToken);
            }

            public Task<ResponseMessage> ReceiveMessageAsync(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                return _connection.ReceiveMessageAsync(responseTo, encoderSelector, messageEncoderSettings, cancellationToken);
            }

            public Task SendMessagesAsync(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                return _connection.SendMessagesAsync(messages, messageEncoderSettings, cancellationToken);
            }
        }

        private sealed class AcquiredConnection : IConnectionHandle
        {
            private ExclusiveConnectionPool _connectionPool;
            private bool _disposed;
            private ReferenceCounted<PooledConnection> _reference;

            public AcquiredConnection(ExclusiveConnectionPool connectionPool, ReferenceCounted<PooledConnection> reference)
            {
                _connectionPool = connectionPool;
                _reference = reference;
            }

            public ConnectionId ConnectionId
            {
                get { return _reference.Instance.ConnectionId; }
            }

            public ConnectionDescription Description
            {
                get { return _reference.Instance.Description; }
            }

            public EndPoint EndPoint
            {
                get { return _reference.Instance.EndPoint; }
            }

            public bool IsExpired
            {
                get
                {
                    return _connectionPool._state.Value == State.Disposed || _reference.Instance.IsExpired;
                }
            }

            public ConnectionSettings Settings
            {
                get { return _reference.Instance.Settings; }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _reference.DecrementReferenceCount();
                    _disposed = true;
                }
            }

            public IConnectionHandle Fork()
            {
                ThrowIfDisposed();
                _reference.IncrementReferenceCount();
                return new AcquiredConnection(_connectionPool, _reference);
            }

            public Task OpenAsync(CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _reference.Instance.OpenAsync(cancellationToken);
            }

            public Task<ResponseMessage> ReceiveMessageAsync(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _reference.Instance.ReceiveMessageAsync(responseTo, encoderSelector, messageEncoderSettings, cancellationToken);
            }

            public Task SendMessagesAsync(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _reference.Instance.SendMessagesAsync(messages, messageEncoderSettings, cancellationToken);
            }

            private void ThrowIfDisposed()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
            }
        }

        private sealed class WaitQueue : IDisposable
        {
            private SemaphoreSlim _semaphore;

            public WaitQueue(int count)
            {
                _semaphore = new SemaphoreSlim(count);
            }

            public int CurrentCount
            {
                get { return _semaphore.CurrentCount; }
            }

            public void Release()
            {
                _semaphore.Release();
            }

            public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
            {
                return _semaphore.Wait(timeout, cancellationToken);
            }

            public Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
            {
                return _semaphore.WaitAsync(timeout, cancellationToken);
            }

            public void Dispose()
            {
                _semaphore.Dispose();
            }
        }

        private class ListConnectionHolder
        {
            private readonly object _lock = new object();
            private readonly List<PooledConnection> _connections;

            private readonly Action<ConnectionPoolBeforeRemovingAConnectionEvent> _beforeRemovingAConnectionEventHandler;
            private readonly Action<ConnectionPoolAfterRemovingAConnectionEvent> _afterRemovingAConnectionEventHandler;

            public ListConnectionHolder(IEventSubscriber eventSubscriber)
            {
                _connections = new List<PooledConnection>();

                eventSubscriber.TryGetEventHandler(out _beforeRemovingAConnectionEventHandler);
                eventSubscriber.TryGetEventHandler(out _afterRemovingAConnectionEventHandler);
            }

            public int Count
            {
                get
                {
                    lock (_lock)
                    {
                        return _connections.Count;
                    }
                }
            }

            public void Clear()
            {
                lock (_lock)
                {
                    foreach (var connection in _connections)
                    {
                        RemoveConnection(connection);
                    }
                    _connections.Clear();
                }
            }

            public void Prune()
            {
                lock (_lock)
                {
                    for (int i = 0; i < _connections.Count; i++)
                    {
                        if (_connections[i].IsExpired)
                        {
                            RemoveConnection(_connections[i]);
                            _connections.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            public PooledConnection Acquire()
            {
                lock (_lock)
                {
                    if (_connections.Count > 0)
                    {
                        var connection = _connections[_connections.Count - 1];
                        _connections.RemoveAt(_connections.Count - 1);
                        if (connection.IsExpired)
                        {
                            RemoveConnection(connection);
                        }
                        else
                        {
                            return connection;
                        }
                    }
                }
                return null;
            }

            public void Return(PooledConnection connection)
            {
                if (connection.IsExpired)
                {
                    RemoveConnection(connection);
                    return;
                }

                lock (_lock)
                {
                    _connections.Add(connection);
                }
            }

            private void RemoveConnection(PooledConnection connection)
            {
                if (_beforeRemovingAConnectionEventHandler != null)
                {
                    _beforeRemovingAConnectionEventHandler(new ConnectionPoolBeforeRemovingAConnectionEvent(connection.ConnectionId));
                }

                var stopwatch = Stopwatch.StartNew();
                connection.Dispose();
                stopwatch.Stop();

                if (_afterRemovingAConnectionEventHandler != null)
                {
                    _afterRemovingAConnectionEventHandler(new ConnectionPoolAfterRemovingAConnectionEvent(connection.ConnectionId, stopwatch.Elapsed));
                }
            }
        }
    }
}
