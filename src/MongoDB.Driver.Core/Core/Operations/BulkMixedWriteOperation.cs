/* Copyright 2010-2015 MongoDB Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents a mixed write bulk operation.
    /// </summary>
    public class BulkMixedWriteOperation : IWriteOperation<BulkWriteOperationResult>
    {
        // fields
        private bool? _bypassDocumentValidation;
        private readonly CollectionNamespace _collectionNamespace;
        private bool _isOrdered = true;
        private int? _maxBatchCount;
        private int? _maxBatchLength;
        private int? _maxDocumentSize;
        private int? _maxWireDocumentSize;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly IEnumerable<WriteRequest> _requests;
        private WriteConcern _writeConcern;
        private bool _retryOnFailure;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BulkMixedWriteOperation"/> class.
        /// </summary>
        /// <param name="collectionNamespace">The collection namespace.</param>
        /// <param name="requests">The requests.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public BulkMixedWriteOperation(
            CollectionNamespace collectionNamespace,
            IEnumerable<WriteRequest> requests,
            MessageEncoderSettings messageEncoderSettings)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _requests = Ensure.IsNotNull(requests, nameof(requests));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
            _writeConcern = WriteConcern.Acknowledged;
        }

        // properties
        /// <summary>
        /// Gets or sets a value indicating whether to bypass document validation.
        /// </summary>
        /// <value>
        /// A value indicating whether to bypass document validation.
        /// </value>
        public bool? BypassDocumentValidation
        {
            get { return _bypassDocumentValidation; }
            set { _bypassDocumentValidation = value; }
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
        /// Gets or sets a value indicating whether the writes must be performed in order.
        /// </summary>
        /// <value>
        /// <c>true</c> if the writes must be performed in order; otherwise, <c>false</c>.
        /// </value>
        public bool IsOrdered
        {
            get { return _isOrdered; }
            set { _isOrdered = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of documents in a batch.
        /// </summary>
        /// <value>
        /// The maximum number of documents in a batch.
        /// </value>
        public int? MaxBatchCount
        {
            get { return _maxBatchCount; }
            set { _maxBatchCount = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the maximum length of a batch.
        /// </summary>
        /// <value>
        /// The maximum length of a batch.
        /// </value>
        public int? MaxBatchLength
        {
            get { return _maxBatchLength; }
            set { _maxBatchLength = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the maximum size of a document.
        /// </summary>
        /// <value>
        /// The maximum size of a document.
        /// </value>
        public int? MaxDocumentSize
        {
            get { return _maxDocumentSize; }
            set { _maxDocumentSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the maximum size of a wire document.
        /// </summary>
        /// <value>
        /// The maximum size of a wire document.
        /// </value>
        public int? MaxWireDocumentSize
        {
            get { return _maxWireDocumentSize; }
            set { _maxWireDocumentSize = Ensure.IsNullOrGreaterThanZero(value, nameof(value)); }
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
        /// Gets the requests.
        /// </summary>
        /// <value>
        /// The requests.
        /// </value>
        public IEnumerable<WriteRequest> Requests
        {
            get { return _requests; }
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
        /// <value>
        /// The write concern.
        /// </value>
        public WriteConcern WriteConcern
        {
            get { return _writeConcern; }
            set { _writeConcern = Ensure.IsNotNull(value, nameof(value)); }
        }

        // public methods
        /// <inheritdoc/>
        public BulkWriteOperationResult Execute(IWriteBinding binding, CancellationToken cancellationToken)
        {
            var helper = new BatchHelper(this);
            BatchHelper.Batch batch = null;

            using (EventContext.BeginOperation())
            using (var channelSource = binding.GetWriteChannelSource(cancellationToken))
            using (var channel = channelSource.GetChannel(cancellationToken))
            {
                do
                {
                    batch = helper.GetNextBatch(channel);
                    var result = ExecuteBatch(channel, binding.Session, batch.Run, batch.IsLast, cancellationToken);
                    helper.AddBatchResult(result);
                }
                while (!batch.IsLast);
            }

            return helper.GetFinalResultOrThrow();
        }

        /// <inheritdoc/>
        public Task<BulkWriteOperationResult> ExecuteAsync(IWriteBinding binding, CancellationToken cancellationToken)
        {
            return null;
            //using (EventContext.BeginOperation())
            //using (var channelSource = await binding.GetWriteChannelSourceAsync(cancellationToken).ConfigureAwait(false))
            //using (var channel = await channelSource.GetChannelAsync(cancellationToken).ConfigureAwait(false))
            //{
            //    var helper = new BatchHelper(this, channel);
            //    foreach (var batch in helper.GetBatches())
            //    {
            //        batch.Result = await ExecuteBatchAsync(channel, binding.Session, batch.Run, batch.IsLast, cancellationToken).ConfigureAwait(false);
            //    }
            //    return helper.GetFinalResultOrThrow();
            //}
        }

        // private methods
        private BulkDeleteOperation CreateDeleteOperation(IEnumerable<DeleteRequest> requests, bool isLast)
        {
            return new BulkDeleteOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                MaxBatchCount = _maxBatchCount,
                MaxBatchLength = _maxBatchLength,
                WriteConcern = GetEffectiveWriteConcern(isLast),
                RetryOnFailure = true
            };
        }

        private BulkInsertOperation CreateInsertOperation(IEnumerable<InsertRequest> requests, bool isLast)
        {
            return new BulkInsertOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                BypassDocumentValidation = _bypassDocumentValidation,
                MaxBatchCount = _maxBatchCount,
                MaxBatchLength = _maxBatchLength,
                IsOrdered = _isOrdered,
                MessageEncoderSettings = _messageEncoderSettings,
                WriteConcern = GetEffectiveWriteConcern(isLast),
                RetryOnFailure = true
            };
        }

        private BulkUpdateOperation CreateUpdateOperation(IEnumerable<UpdateRequest> requests, bool isLast)
        {
            return new BulkUpdateOperation(_collectionNamespace, requests, _messageEncoderSettings)
            {
                BypassDocumentValidation = _bypassDocumentValidation,
                MaxBatchCount = _maxBatchCount,
                MaxBatchLength = _maxBatchLength,
                IsOrdered = _isOrdered,
                WriteConcern = GetEffectiveWriteConcern(isLast),
                RetryOnFailure = true
            };
        }

        private BulkWriteBatchResult ExecuteBatch(IChannelHandle channel, ICoreSessionHandle session, Run run, bool isLast, CancellationToken cancellationToken)
        {
            BulkWriteOperationResult result;
            MongoBulkWriteOperationException exception = null;
            try
            {
                switch (run.RequestType)
                {
                    case WriteRequestType.Delete:
                        result = ExecuteDeletes(channel, session, run.Requests.Cast<DeleteRequest>(), isLast, cancellationToken);
                        break;
                    case WriteRequestType.Insert:
                        result = ExecuteInserts(channel, session, run.Requests.Cast<InsertRequest>(), isLast, cancellationToken);
                        break;
                    case WriteRequestType.Update:
                        result = ExecuteUpdates(channel, session, run.Requests.Cast<UpdateRequest>(), isLast, cancellationToken);
                        break;
                    default:
                        throw new MongoInternalException("Unrecognized RequestType.");
                }
            }
            catch (MongoBulkWriteOperationException ex)
            {
                result = ex.Result;
                exception = ex;
            }

            return BulkWriteBatchResult.Create(result, exception, run.IndexMap);
        }

        private async Task<BulkWriteBatchResult> ExecuteBatchAsync(IChannelHandle channel, ICoreSessionHandle session, Run run, bool isLast, CancellationToken cancellationToken)
        {
            BulkWriteOperationResult result;
            MongoBulkWriteOperationException exception = null;
            try
            {
                switch (run.RequestType)
                {
                    case WriteRequestType.Delete:
                        result = await ExecuteDeletesAsync(channel, session, run.Requests.Cast<DeleteRequest>(), isLast, cancellationToken).ConfigureAwait(false);
                        break;
                    case WriteRequestType.Insert:
                        result = await ExecuteInsertsAsync(channel, session, run.Requests.Cast<InsertRequest>(), isLast, cancellationToken).ConfigureAwait(false);
                        break;
                    case WriteRequestType.Update:
                        result = await ExecuteUpdatesAsync(channel, session, run.Requests.Cast<UpdateRequest>(), isLast, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        throw new MongoInternalException("Unrecognized RequestType.");
                }
            }
            catch (MongoBulkWriteOperationException ex)
            {
                result = ex.Result;
                exception = ex;
            }

            return BulkWriteBatchResult.Create(result, exception, run.IndexMap);
        }

        private BulkWriteOperationResult ExecuteDeletes(IChannelHandle channel, ICoreSessionHandle session, IEnumerable<DeleteRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateDeleteOperation(requests, isLast);
            return operation.Execute(channel, session, cancellationToken);
        }

        private Task<BulkWriteOperationResult> ExecuteDeletesAsync(IChannelHandle channel, ICoreSessionHandle session, IEnumerable<DeleteRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateDeleteOperation(requests, isLast);
            return operation.ExecuteAsync(channel, session, cancellationToken);
        }

        private BulkWriteOperationResult ExecuteInserts(IChannelHandle channel, ICoreSessionHandle session, IEnumerable<InsertRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateInsertOperation(requests, isLast);
            return operation.Execute(channel, session, cancellationToken);
        }

        private Task<BulkWriteOperationResult> ExecuteInsertsAsync(IChannelHandle channel, ICoreSessionHandle session, IEnumerable<InsertRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateInsertOperation(requests, isLast);
            return operation.ExecuteAsync(channel, session, cancellationToken);
        }

        private BulkWriteOperationResult ExecuteUpdates(IChannelHandle channel, ICoreSessionHandle session, IEnumerable<UpdateRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateUpdateOperation(requests, isLast);
            return operation.Execute(channel,session, cancellationToken);
        }

        private Task<BulkWriteOperationResult> ExecuteUpdatesAsync(IChannelHandle channel, ICoreSessionHandle session, IEnumerable<UpdateRequest> requests, bool isLast, CancellationToken cancellationToken)
        {
            var operation = CreateUpdateOperation(requests, isLast);
            return operation.ExecuteAsync(channel, session, cancellationToken);
        }

        private WriteConcern GetEffectiveWriteConcern(bool isLast)
        {
            if (_isOrdered && !isLast && !_writeConcern.IsAcknowledged)
            {
                return WriteConcern.W1; // explicitly do not use the server's default.
            }

            return _writeConcern;
        }

        // nested types
        private class BatchHelper
        {
            private readonly List<BulkWriteBatchResult> _batchResults = new List<BulkWriteBatchResult>();
            private bool _hasWriteErrors;
            private readonly BulkMixedWriteOperation _operation;
            private List<SavedWriteRequest> _savedRequests = new List<SavedWriteRequest>();
            private ConnectionId _lastConnectionId;

            private ReadAheadEnumerable<WriteRequest>.ReadAheadEnumerator _requests;
            private int _requestIndex;

            public BatchHelper(BulkMixedWriteOperation operation)
            {
                _operation = operation;
                _requestIndex = -1;
            }

            public void AddBatchResult(BulkWriteBatchResult result)
            {
                _batchResults.Add(result);
                _hasWriteErrors |= result.HasWriteErrors;
            }

            public Batch GetNextBatch(IChannelHandle channel)
            {
                _lastConnectionId = channel.ConnectionDescription.ConnectionId;
                var maxRunLength = Math.Min(_operation._maxBatchCount ?? int.MaxValue, channel.ConnectionDescription.MaxBatchCount);
                if (_requests == null)
                {
                    _requests = new ReadAheadEnumerable<WriteRequest>.ReadAheadEnumerator(_operation._requests.GetEnumerator());
                }

                Run r = new Run();
                ReadFromSavedRequests(r, maxRunLength);
                if(_savedRequests.Count == 0)
                {
                    ReadFromRequests(r, maxRunLength);
                }

                if (_batchResults.Count == 0 && r.Count == 0)
                {
                    throw new InvalidOperationException("Bulk write operation is empty.");
                }

                return new Batch
                {
                    IsLast = !_requests.HasNext && _savedRequests.Count == 0,
                    Run = r
                };
            }

            public BulkWriteOperationResult GetFinalResultOrThrow()
            {
                var combiner = new BulkWriteBatchResultCombiner(_batchResults, _operation._writeConcern.IsAcknowledged);
                return combiner.CreateResultOrThrowIfHasErrors(_lastConnectionId, _savedRequests.ToList());
            }

            private void ReadFromRequests(Run r, int maxRunLength)
            {
                while (r.Count < maxRunLength && _requests.MoveNext())
                {
                    _requestIndex++;
                    if (r.Count == 0)
                    {
                        r.Add(_requests.Current, _requestIndex);
                    }
                    else if (r.RequestType == _requests.Current.RequestType)
                    {
                        r.Add(_requests.Current, _requestIndex);
                    }
                    else
                    {
                        _savedRequests.Add(new SavedWriteRequest { Request = _requests.Current, Index = _requestIndex });
                        if (_operation.IsOrdered)
                        {
                            break;
                        }
                    }
                }
            }

            private void ReadFromSavedRequests(Run r, int maxRunLength)
            {
                int remainingRequestIndex = 0;
                while (r.Count < maxRunLength && _savedRequests.Count > remainingRequestIndex)
                {
                    var savedRequest = _savedRequests[remainingRequestIndex];
                    if (r.Count == 0)
                    {
                        r.Add(savedRequest.Request, savedRequest.Index);
                        _savedRequests.RemoveAt(remainingRequestIndex);
                    }
                    else if (savedRequest.Request.RequestType == r.RequestType)
                    {
                        r.Add(savedRequest.Request, savedRequest.Index);
                        _savedRequests.RemoveAt(remainingRequestIndex);
                    }
                    else
                    {
                        if (_operation.IsOrdered)
                        {
                            break;
                        }
                        remainingRequestIndex++;
                    }
                }
            }

            private class SavedWriteRequest
            {
                public WriteRequest Request;
                public int Index;
            }

            public class Batch
            {
                public bool IsLast;
                public BulkWriteBatchResult Result;
                public Run Run;
            }
        }

        private class Run
        {
            // fields
            private IndexMap _indexMap = new IndexMap.RangeBased();
            private readonly List<WriteRequest> _requests = new List<WriteRequest>();

            // properties
            public int Count
            {
                get { return _requests.Count; }
            }

            public IndexMap IndexMap
            {
                get { return _indexMap; }
            }

            public List<WriteRequest> Requests
            {
                get { return _requests; }
            }

            public WriteRequestType RequestType
            {
                get { return _requests[0].RequestType; }
            }

            // methods
            public void Add(WriteRequest request, int originalIndex)
            {
                var index = _requests.Count;
                _indexMap = _indexMap.Add(index, originalIndex);
                _requests.Add(request);
            }
        }
    }
}
