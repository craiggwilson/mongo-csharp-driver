using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Operations
{
    internal static class OperationHelper
    {
        public static bool IsRetryableWrite(bool retryOnFailure, WriteConcern writeConcern, ConnectionDescription connectionDescription)
        {
            if (!retryOnFailure)
            {
                return false;
            }

            if (!writeConcern.IsAcknowledged)
            {
                return false;
            }

            return CanRetryWrite(connectionDescription);

        }

        private static bool CanRetryWrite(ConnectionDescription connectionDescription)
        {
            if (connectionDescription.IsMasterResult.LogicalSessionTimeout == null)
            {
                return false;
            }

            if (connectionDescription.IsMasterResult.ServerType == Servers.ServerType.Standalone)
            {
                return false;
            }

            return true;
        }

        public static bool IsRetryableException(Exception ex)
        {
            return ex is MongoNotPrimaryException ||
                ex is MongoConnectionException ||
                ex is MongoNodeIsRecoveringException;
        }
    }
}
