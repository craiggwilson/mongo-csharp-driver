using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Configuration;

namespace MongoDB.AdoNet
{
    public sealed class MongoConnection : DbConnection
    {
        private MongoInternalConnection _internalConnection;
        private ConnectionString _connectionString;

        public MongoConnection()
        { }

        public MongoConnection(string connectionString)
        {
            _connectionString = new ConnectionString(connectionString);
        }

        public override string ConnectionString
        {
            get { return _connectionString.ToString(); }
            set { _connectionString = value == null ? null : new ConnectionString(value); }
        }

        public override string DataSource
        {
            get
            {
                if (_connectionString != null)
                {
                    return string.Join(", ", _connectionString.Hosts.Select(x => x.ToString()));
                }
                return null;
            }
        }

        public override string Database
        {
            get
            {
                if (_connectionString != null)
                {
                    return _connectionString.DatabaseName;
                }
                return null;
            }
        }

        public override string ServerVersion
        {
            get
            {
                return GetOpenConnection().ServerVersion;
            }
        }

        public override ConnectionState State
        {
            get
            {
                return GetOpenConnection().State;
            }
        }

        internal MongoInternalConnection InternalConnection
        {
            get { return GetOpenConnection(); }
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            _internalConnection = null;
        }

        public override void Open()
        {
            if (_internalConnection != null)
            {
                _internalConnection = MongoInternalConnection.GetOpenConnection(_connectionString);
            }
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotSupportedException("Transactions are not supported in the MongoDB provider.");
        }

        protected override DbCommand CreateDbCommand()
        {
            return new MongoCommand(this);
        }

        private MongoInternalConnection GetOpenConnection()
        {
            if (_internalConnection == null)
            {
                throw new InvalidOperationException("Connection must be opened.");
            }
            return _internalConnection;
        }
    }
}
