using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Bindings
{
    internal sealed class PipelineBinding : IReadWriteBinding
    {
        private bool _disposed;
        private readonly ConcurrentDictionary<ServerId, PipeliningChannelSource> _cache;
        private readonly IReadBinding _readBinding;
        private readonly IWriteBinding _writeBinding;

        public PipelineBinding(IReadBinding readBinding, IWriteBinding writeBinding, int maxChannelsPerServer)
        {
            _readBinding = Ensure.IsNotNull(readBinding, nameof(readBinding));
            _writeBinding = Ensure.IsNotNull(writeBinding, nameof(writeBinding));

            _cache = new ConcurrentDictionary<ServerId, PipeliningChannelSource>();
        }

        /// <inheritdoc/>
        public ReadPreference ReadPreference => _readBinding.ReadPreference;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var source in _cache.Values)
                {
                    source.Dispose();
                }
                _cache.Clear();
                _disposed = true;
            }
        }

        /// <inheritdoc/>
        public IChannelSourceHandle GetReadChannelSource(CancellationToken cancellationToken)
        {
            var source = _readBinding.GetReadChannelSource(cancellationToken);
            return new ChannelSourceHandle(_cache.GetOrAdd(
                source.Server.ServerId,
                id => new PipeliningChannelSource(source, 2)));
        }

        /// <inheritdoc/>
        public async Task<IChannelSourceHandle> GetReadChannelSourceAsync(CancellationToken cancellationToken)
        {
            var source = await _readBinding.GetReadChannelSourceAsync(cancellationToken).ConfigureAwait(false);
            return new ChannelSourceHandle(_cache.GetOrAdd(
                source.Server.ServerId,
                id => new PipeliningChannelSource(source, 2)));
        }

        /// <inheritdoc/>
        public IChannelSourceHandle GetWriteChannelSource(CancellationToken cancellationToken)
        {
            var source = _writeBinding.GetWriteChannelSource(cancellationToken);
            return new ChannelSourceHandle(_cache.GetOrAdd(
                source.Server.ServerId,
                id => new PipeliningChannelSource(source, 2)));
        }

        /// <inheritdoc/>
        public async Task<IChannelSourceHandle> GetWriteChannelSourceAsync(CancellationToken cancellationToken)
        {
            var source = await _writeBinding.GetWriteChannelSourceAsync(cancellationToken).ConfigureAwait(false);
            return new ChannelSourceHandle(_cache.GetOrAdd(
                source.Server.ServerId,
                id => new PipeliningChannelSource(source, 2)));
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private class PipeliningChannelSource : IChannelSource
        {
            private readonly List<IChannelHandle> _channels;
            private readonly IChannelSource _channelSource;
            private int _currentIndex;
            private bool _disposed;
            private readonly SemaphoreSlim _lock;
            private readonly int _maxChannels;

            public PipeliningChannelSource(IChannelSource channelSource, int maxChannels)
            {
                _channelSource = channelSource;

                _maxChannels = maxChannels;
                _currentIndex = -1;
                _channels = new List<IChannelHandle>();
                _lock = new SemaphoreSlim(1);
            }

            public IServer Server => _channelSource.Server;

            public ServerDescription ServerDescription => _channelSource.ServerDescription;

            public void Dispose()
            {
                if (!_disposed)
                {
                    _channels.ForEach(c => c.Dispose());
                    _channels.Clear();
                    _channelSource.Dispose();
                    _lock.Dispose();
                    _disposed = true;
                }
            }

            public IChannelHandle GetChannel(CancellationToken cancellationToken)
            {
                _lock.Wait(cancellationToken);
                try
                {
                    _currentIndex++;
                    if (_currentIndex < _channels.Count)
                    {
                        return _channels[_currentIndex];
                    }

                    if (_channels.Count < _maxChannels)
                    {
                        var channel = _channelSource.GetChannel(cancellationToken);
                        _channels.Add(channel);
                        return channel;
                    }

                    _currentIndex = 0;
                    return _channels[0];
                }
                finally
                {
                    _lock.Release();
                }
            }

            public async Task<IChannelHandle> GetChannelAsync(CancellationToken cancellationToken)
            {
                await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    _currentIndex++;
                    if (_currentIndex < _channels.Count)
                    {
                        return _channels[_currentIndex];
                    }

                    if (_channels.Count < _maxChannels)
                    {
                        var channel = await _channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false);
                        _channels.Add(channel);
                        return channel;
                    }

                    _currentIndex = 0;
                    return _channels[0];
                }
                finally
                {
                    _lock.Release();
                }
            }

            private void ThrowIfDisposed()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
            }
        }
    }
}
