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
using System.Net;
using System.Reflection;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.TestHelpers.XunitExtensions;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;
using Xunit;

namespace MongoDB.Driver.Core.Operations
{
    public class OperationHelperTests : OperationTestBase
    {
        [Theory]
        [InlineData(true, true, 10, ServerType.ReplicaSetPrimary, true)]
        [InlineData(false, true, 10, ServerType.ReplicaSetPrimary, false)]
        [InlineData(true, false, 10, ServerType.ReplicaSetPrimary, false)]
        [InlineData(true, true, null, ServerType.ReplicaSetPrimary, false)]
        [InlineData(true, true, 10, ServerType.Standalone, false)]
        public void IsRetryable_should_return_the_correct_result(
            bool retryOnFailure,
            bool acknowledged,
            int? logicalSessionTimeout,
            ServerType serverType,
            bool expectedResult)
        {
            var result = OperationHelper.IsRetryableWrite(
                retryOnFailure,
                acknowledged ? WriteConcern.Acknowledged : WriteConcern.Unacknowledged,
                CreateConnectionDescription(logicalSessionTimeout, serverType));

            result.Should().Be(expectedResult);
        }

        [Theory]
        [MemberData("RetryableExceptions")]
        public void IsRetryableException_should_return_the_correct_result(Exception ex, bool expectedResult)
        {
            var result = OperationHelper.IsRetryableException(ex);
            result.Should().Be(expectedResult);
        }

        public static IEnumerable<object[]> RetryableExceptions()
        {
            var connectionId = CreateConnectionId();

            yield return new object[] { new MongoConnectionException(connectionId, "hlah"), true };
            yield return new object[] { new MongoConnectionClosedException(connectionId), true };
            yield return new object[] { new MongoNotPrimaryException(connectionId, new BsonDocument()), true };
            yield return new object[] { new MongoNodeIsRecoveringException(connectionId, new BsonDocument()), true };
            yield return new object[] { new Exception(), false };
            yield return new object[] { new MongoCommandException(connectionId, "afl", new BsonDocument()), false };
        }

        private static ConnectionId CreateConnectionId()
        {
            var clusterId = new ClusterId();
            var serverId = new ServerId(clusterId, new DnsEndPoint("localhost", 27017));
            return new ConnectionId(serverId);
        }

        private static ConnectionDescription CreateConnectionDescription(int? logicalSessionTimeout, ServerType serverType)
        {
            var connectionId = CreateConnectionId();
            var buildInfoResult = new BuildInfoResult(new BsonDocument("ok", 1).Add("version", "0.0.0"));
            var isMasterResult = CreateIsMasterResult(logicalSessionTimeout, serverType);

            return new ConnectionDescription(connectionId, isMasterResult, buildInfoResult);
        }

        private static IsMasterResult CreateIsMasterResult(int? logicalSessionTimeout, ServerType serverType)
        {
            var isMasterDocument = BsonDocument.Parse("{ ok: 1 }");
            if (logicalSessionTimeout != null)
            {
                isMasterDocument.Add("logicalSessionTimeoutMinutes", 10);
            }
            switch (serverType)
            {
                case ServerType.ReplicaSetArbiter:
                    isMasterDocument.Add("setName", "rs");
                    isMasterDocument.Add("arbiterOnly", true);
                    break;
                case ServerType.ReplicaSetGhost:
                    isMasterDocument.Add("isreplicaset", true);
                    break;
                case ServerType.ReplicaSetOther:
                    isMasterDocument.Add("setName", "rs");
                    break;
                case ServerType.ReplicaSetPrimary:
                    isMasterDocument.Add("setName", "rs");
                    isMasterDocument.Add("ismaster", true);
                    break;
                case ServerType.ReplicaSetSecondary:
                    isMasterDocument.Add("setName", "rs");
                    isMasterDocument.Add("secondary", true);
                    break;
                case ServerType.ShardRouter:
                    isMasterDocument.Add("msg", "isdbgrid");
                    break;
            }
            return new IsMasterResult(isMasterDocument);
        }
    }
}
