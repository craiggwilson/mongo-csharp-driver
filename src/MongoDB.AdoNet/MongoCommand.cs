using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.AdoNet
{
    public class MongoCommand : DbCommand
    {
        private MongoConnection _connection;
        private string _commandText;
        private TimeSpan _commandTimeout;
        private CommandType _commandType;
        private bool _designTimeVisible;
        private UpdateRowSource _updateRowSource;

        public MongoCommand()
        {
            _commandTimeout = TimeSpan.FromSeconds(30);
        }

        public MongoCommand(MongoConnection connection)
            : this()
        {
            _connection = connection;
        }

        public override string CommandText
        {
            get { return _commandText; }
            set { _commandText = value; }
        }

        public override int CommandTimeout
        {
            get { return _commandTimeout.Milliseconds; }
            set { _commandTimeout = TimeSpan.FromMilliseconds(value); }
        }

        public override CommandType CommandType
        {
            get { return _commandType; }
            set { _commandType = value; }
        }

        public override bool DesignTimeVisible
        {
            get { return _designTimeVisible; }
            set { _designTimeVisible = value; }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _updateRowSource; }
            set { _updateRowSource = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return _connection; }
            set
            {
                if (!(value is MongoConnection))
                {
                    throw new InvalidOperationException("Only MongoConnection can be used with a MongoCommand.");
                }
                _connection = (MongoConnection)value;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { throw new NotSupportedException("Parameters are not supported in the MongoDB provider."); }
        }

        protected override DbTransaction DbTransaction
        {
            get { throw new NotSupportedException("Transactions are not supported in the MongoDB provider."); }
            set { throw new NotSupportedException("Transactions are not supported in the MongoDB provider."); }
        }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            // nothing to do
        }

        protected override DbParameter CreateDbParameter()
        {
            throw new NotSupportedException("Parameters are not supported in the MongoDB provider.");
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }
    }
}
