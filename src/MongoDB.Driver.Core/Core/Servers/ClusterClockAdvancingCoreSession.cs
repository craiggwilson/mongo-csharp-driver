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

using MongoDB.Bson;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;

namespace MongoDB.Driver.Core.Servers
{
    internal  sealed class ClusterClockAdvancingCoreSession : ICoreSession
    {
        private readonly IClusterClock _clusterClock;
        private readonly ICoreSession _wrapped;

        public ClusterClockAdvancingCoreSession(ICoreSession wrapped, IClusterClock clusterClock)
        {
            _wrapped = wrapped;
            _clusterClock = clusterClock;
        }

        public BsonDocument ClusterTime => ClusterClock.GreaterClusterTime(_wrapped.ClusterTime, _clusterClock.ClusterTime);

        public BsonDocument Id => _wrapped.Id;

        public bool IsImplicitSession => _wrapped.IsImplicitSession;

        public BsonTimestamp OperationTime => _wrapped.OperationTime;

        public void AdvanceClusterTime(BsonDocument newClusterTime)
        {
            _wrapped.AdvanceClusterTime(newClusterTime);
            _clusterClock.AdvanceClusterTime(newClusterTime);
        }

        public void AdvanceOperationTime(BsonTimestamp newOperationTime)
        {
            _wrapped.AdvanceOperationTime(newOperationTime);
        }

        public void WasUsed()
        {
            _wrapped.WasUsed();
        }
    }
}
