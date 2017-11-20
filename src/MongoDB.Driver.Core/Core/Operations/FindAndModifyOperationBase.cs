/* Copyright 2013-2017 MongoDB Inc.
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a base class for find and modify operations.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class FindAndModifyOperationBase<TResult> : IWriteOperation<TResult>
    {
        // fields
        private Collation _collation;
        private readonly CollectionNamespace _collectionNamespace;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly IBsonSerializer<TResult> _resultSerializer;
        private WriteConcern _writeConcern;
        private bool _retryOnFailure;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FindAndModifyOperationBase{TResult}"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public FindAndModifyOperationBase(CollectionNamespace collectionNamespace, IBsonSerializer<TResult> resultSerializer, MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
        }

        // properties
        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        /// <value>
        /// The collation.
        /// </value>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        /// <value>
        /// The collection namespace.
        /// </value>
        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
        }

        /// <summary>
        /// Gets the message encoder settings.
        /// </summary>
        /// <value>
        /// The message encoder settings.
        /// </value>
        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        /// <value>
        /// The result serializer.
        /// </value>
        public IBsonSerializer<TResult> ResultSerializer
        {
            get { return _resultSerializer; }
        }

        /// <summary>
        /// Gets or sets whether to retry the operation on failure.
        /// </summary>
        public bool RetryOnFailure
        {
            get { return _retryOnFailure; }
            set { _retryOnFailure = value; }
        }

        /// <summary>
        /// Gets or sets the write concern.
        /// </summary>
        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = value; }
        }

        // public methods
        /// <inheritdoc/>
        public TResult Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            return Execute(binding, null, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<TResult> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            return ExecuteAsync(binding, null, cancellationToken);
        }

        // private methods
        internal abstract BsonDocument CreateCommand(SemanticVersion serverVersion, long? transactionId);

        private WriteCommandOperation<RawBsonDocument> CreateOperation(SemanticVersion serverVersion, long? transactionId)
        {
            var command = CreateCommand(serverVersion, transactionId);
            return new WriteCommandOperation<RawBsonDocument>(_collectionNamespace.DatabaseNamespace, command, RawBsonDocumentSerializer.Instance, _messageEncoderSettings)
            {
                CommandValidator = GetCommandValidator()
            };
        }

        private TResult Execute(IWriteBinding binding, long? transactionId, CancellationToken cancellationToken)
        {
            bool retryable = false;
            try
            {
                using (var channelSource = binding.GetWriteChannelSource(cancellationToken))
                using (var channel = channelSource.GetChannel(cancellationToken))
                using (var channelBinding = new ChannelReadWriteBinding(channelSource.Server, channel, binding.Session.Fork()))
                {
                    if (transactionId == null && OperationHelper.IsRetryableWrite(_retryOnFailure, _writeConcern, channel.ConnectionDescription))
                    {
                        transactionId = binding.Session.AdvanceTransactionId();
                        retryable = true;
                    }
                    var operation = CreateOperation(channel.ConnectionDescription.ServerVersion, transactionId);
                    using (var rawBsonDocument = operation.Execute(channelBinding, cancellationToken))
                    {
                        return ProcessCommandResult(channel.ConnectionDescription.ConnectionId, rawBsonDocument);
                    }
                }
            }
            catch(Exception ex) when(retryable && OperationHelper.IsRetryableException(ex))
            {
                try
                {
                    return Execute(binding, transactionId, cancellationToken);
                }
                catch (Exception retryEx) when (OperationHelper.IsRetryableException(retryEx))
                {
                    throw;
                }
                catch { }
                { }
                throw; // ignore nested exception and throw outer exception
            }
        }

        private async Task<TResult> ExecuteAsync(IWriteBinding binding, long? transactionId, CancellationToken cancellationToken)
        {
            bool retryable = false;
            try
            {
                using (var channelSource = await binding.GetWriteChannelSourceAsync(cancellationToken).ConfigureAwait(false))
                using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
                using (var channelBinding = new ChannelReadWriteBinding(channelSource.Server, channel, binding.Session.Fork()))
                {
                    if (transactionId == null && OperationHelper.IsRetryableWrite(_retryOnFailure, _writeConcern, channel.ConnectionDescription))
                    {
                        transactionId = binding.Session.AdvanceTransactionId();
                        retryable = true;
                    }
                    var operation = CreateOperation(channel.ConnectionDescription.ServerVersion, transactionId);
                    using (var rawBsonDocument = await operation.ExecuteAsync(channelBinding, cancellationToken).ConfigureAwait(false))
                    {
                        return ProcessCommandResult(channel.ConnectionDescription.ConnectionId, rawBsonDocument);
                    }
                }
            }
            catch (Exception ex) when (retryable && OperationHelper.IsRetryableException(ex))
            {
                try
                {
                    return await ExecuteAsync(binding, transactionId, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception retryEx) when (OperationHelper.IsRetryableException(retryEx))
                {
                    throw;
                }
                catch { }
                throw; // ignore nested exception and throw outer exception
            }
        }

        /// <summary>
        /// Gets the command validator.
        /// </summary>
        /// <returns>An element name validator for the command.</returns>
        protected abstract IElementNameValidator GetCommandValidator();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private TResult ProcessCommandResult(ConnectionId connectionId, RawBsonDocument rawBsonDocument)
        {
            var binaryReaderSettings = new BsonBinaryReaderSettings
            {
                Encoding = _messageEncoderSettings.GetOrDefault<UTF8Encoding>(MessageEncoderSettingsName.ReadEncoding, Utf8Encodings.Strict),
                GuidRepresentation = _messageEncoderSettings.GetOrDefault<GuidRepresentation>(MessageEncoderSettingsName.GuidRepresentation, GuidRepresentation.CSharpLegacy)
            };

            BsonValue writeConcernError;
            if (rawBsonDocument.TryGetValue("writeConcernError", out writeConcernError))
            {
                var message = writeConcernError["errmsg"].AsString;
                var response = rawBsonDocument.Materialize(binaryReaderSettings);
                var writeConcernResult = new WriteConcernResult(response);
                throw new MongoWriteConcernException(connectionId, message, writeConcernResult);
            }

            using (var stream = new ByteBufferStream(rawBsonDocument.Slice, ownsBuffer: false))
            using (var reader = new BsonBinaryReader(stream, binaryReaderSettings))
            {
                var context = BsonDeserializationContext.CreateRoot(reader);
                return _resultSerializer.Deserialize(context);
            }
        }
    }
}
