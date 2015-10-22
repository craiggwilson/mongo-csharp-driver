using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
    internal sealed class NonExclusiveConnectionPool : IConnectionPool
    {
        // fields
        private readonly ConnectionHolder _connectionHolder;
        private readonly IConnectionFactory _connectionFactory;
        private readonly EndPoint _endPoint;
        private int _generation;
        private readonly ServerId _serverId;
        private readonly ConnectionPoolSettings _settings;
        private readonly InterlockedInt32 _state;

        public NonExclusiveConnectionPool(
            ServerId serverId,
            EndPoint endPoint,
            ConnectionPoolSettings settings,
            IConnectionFactory connectionFactory,
            IEventSubscriber eventSubscriber)
        {
            _serverId = Ensure.IsNotNull(serverId, nameof(serverId));
            _endPoint = Ensure.IsNotNull(endPoint, nameof(endPoint));
            _settings = Ensure.IsNotNull(settings, nameof(settings));
            _connectionFactory = Ensure.IsNotNull(connectionFactory, nameof(connectionFactory));
            Ensure.IsNotNull(eventSubscriber, nameof(eventSubscriber));

            _connectionHolder = new ConnectionHolder();
            _state = new InterlockedInt32(State.Initial);
        }

        public int Generation => Interlocked.CompareExchange(ref _generation, 0, 0);

        public ServerId ServerId => _serverId;

        public IConnectionHandle AcquireConnection(CancellationToken cancellationToken)
        {
            ThrowIfNotOpen();

            var connection = GetOrCreateConnection();
            return new AcquiredConnection(this, connection);
        }

        public Task<IConnectionHandle> AcquireConnectionAsync(CancellationToken cancellationToken)
        {
            ThrowIfNotOpen();

            var connection = GetOrCreateConnection();
            return Task.FromResult<IConnectionHandle>(new AcquiredConnection(this, connection));
        }

        public void Clear()
        {
            ThrowIfNotOpen();
            Interlocked.Increment(ref _generation);
        }

        public void Dispose()
        {
            if (_state.TryChange(State.Disposed))
            {
                _connectionHolder.Clear();
            }
        }

        public void Initialize()
        {
            ThrowIfDisposed();
            if (_state.TryChange(State.Initial, State.Open))
            {
                // create the initial number of connections
                for (int i = 0; i < _settings.MinConnections; i++)
                {
                    CreateConnection();
                }
            }
        }

        private PooledConnection CreateConnection()
        {
            var connection = _connectionFactory.CreateConnection(_serverId, _endPoint);
            var pooledConnection = new PooledConnection(this, connection);
            _connectionHolder.Add(pooledConnection);
            return pooledConnection;
        }

        private PooledConnection GetOrCreateConnection()
        {
            var connection = _connectionHolder.Acquire();
            if (connection == null)
            {
                connection = CreateConnection();
            }

            return connection;
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
            private readonly NonExclusiveConnectionPool _connectionPool;
            private readonly int _generation;
            private int _usageCount;

            public PooledConnection(NonExclusiveConnectionPool connectionPool, IConnection connection)
            {
                _connectionPool = connectionPool;
                _connection = connection;
                _generation = connectionPool._generation;
                _usageCount = 0;
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

            public int UsageCount
            {
                get { return Interlocked.CompareExchange(ref _usageCount, 0, 0); }
            }

            public void Dispose()
            {
                _connection.Dispose();
            }

            public void DecrementUsageCount()
            {
                Interlocked.Decrement(ref _usageCount);
            }

            public void IncrementUsageCount()
            {
                Interlocked.Increment(ref _usageCount);
            }

            public void Open(CancellationToken cancellationToken)
            {
                _connection.Open(cancellationToken);
            }

            public Task OpenAsync(CancellationToken cancellationToken)
            {
                return _connection.OpenAsync(cancellationToken);
            }

            public ResponseMessage ReceiveMessage(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                return _connection.ReceiveMessage(responseTo, encoderSelector, messageEncoderSettings, cancellationToken);
            }

            public Task<ResponseMessage> ReceiveMessageAsync(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                return _connection.ReceiveMessageAsync(responseTo, encoderSelector, messageEncoderSettings, cancellationToken);
            }

            public void SendMessages(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                _connection.SendMessages(messages, messageEncoderSettings, cancellationToken);
            }

            public Task SendMessagesAsync(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                return _connection.SendMessagesAsync(messages, messageEncoderSettings, cancellationToken);
            }
        }

        private sealed class AcquiredConnection : IConnectionHandle
        {
            private NonExclusiveConnectionPool _connectionPool;
            private bool _disposed;
            private PooledConnection _reference;

            public AcquiredConnection(NonExclusiveConnectionPool connectionPool, PooledConnection reference)
            {
                _connectionPool = connectionPool;
                _reference = reference;
                _reference.IncrementUsageCount();
            }

            public ConnectionId ConnectionId
            {
                get { return _reference.ConnectionId; }
            }

            public ConnectionDescription Description
            {
                get { return _reference.Description; }
            }

            public EndPoint EndPoint
            {
                get { return _reference.EndPoint; }
            }

            public bool IsExpired
            {
                get
                {
                    return _connectionPool._state.Value == State.Disposed || _reference.IsExpired;
                }
            }

            public ConnectionSettings Settings
            {
                get { return _reference.Settings; }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _reference.DecrementUsageCount();
                    _disposed = true;
                }
            }

            public IConnectionHandle Fork()
            {
                ThrowIfDisposed();
                return new AcquiredConnection(_connectionPool, _reference);
            }

            public void Open(CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                _reference.Open(cancellationToken);
            }

            public Task OpenAsync(CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _reference.OpenAsync(cancellationToken);
            }

            public Task<ResponseMessage> ReceiveMessageAsync(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _reference.ReceiveMessageAsync(responseTo, encoderSelector, messageEncoderSettings, cancellationToken);
            }

            public ResponseMessage ReceiveMessage(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _reference.ReceiveMessage(responseTo, encoderSelector, messageEncoderSettings, cancellationToken);
            }

            public void SendMessages(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                _reference.SendMessages(messages, messageEncoderSettings, cancellationToken);
            }

            public Task SendMessagesAsync(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _reference.SendMessagesAsync(messages, messageEncoderSettings, cancellationToken);
            }

            private void ThrowIfDisposed()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
            }
        }

        private class ConnectionHolder
        {
            private readonly object _lock = new object();
            private readonly LinkedList<PooledConnection> _connections;

            public ConnectionHolder()
            {
                _connections = new LinkedList<PooledConnection>();
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

            public PooledConnection Acquire()
            {
                lock (_lock)
                {
                    if (_connections.Count > 0)
                    {
                        var connection = _connections.First;
                        _connections.RemoveFirst();
                        _connections.AddLast(connection);

                        if (connection.Value.IsExpired)
                        {
                            _connections.RemoveLast();
                            RemoveConnection(connection.Value);
                        }
                        else
                        {
                            return connection.Value;
                        }
                    }
                }
                return null;
            }

            public void Add(PooledConnection connection)
            {
                lock (_lock)
                {
                    _connections.AddLast(connection);
                }
            }

            private void RemoveConnection(PooledConnection connection)
            {
                connection.Dispose();
            }
        }
    }
}
