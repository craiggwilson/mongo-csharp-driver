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

namespace MongoDB.Driver
{
    internal class ClientSessionWrappingCoreSession : ICoreSession
    {
        private readonly IClientSession _clientSession;

        public ClientSessionWrappingCoreSession(IClientSession clientSession)
        {
            _clientSession = clientSession;
        }

        public BsonDocument ClusterTime => _clientSession.ClusterTime;

        public BsonDocument Id => _clientSession.ServerSession.Id;

        public bool IsImplicitSession => _clientSession.IsImplicitSession;

        public BsonTimestamp OperationTime => _clientSession.OperationTime;

        public void AdvanceClusterTime(BsonDocument newClusterTime)
        {
            _clientSession.AdvanceClusterTime(newClusterTime);
        }

        public void AdvanceOperationTime(BsonTimestamp newOperationTime)
        {
            _clientSession.AdvanceOperationTime(newOperationTime);
        }

        public void WasUsed()
        {
            _clientSession.ServerSession.WasUsed();
        }
    }
}
