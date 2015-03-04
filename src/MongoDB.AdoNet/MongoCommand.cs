using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;
using MongoDB.Query.Structure;
using MongoDB.Query.Structure.Parsing;

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
            if(_connection == null)
            {
                throw new InvalidOperationException("A connection has not been provided.");
            }

            var pipeline = new Parser(new Lexer(_commandText)).Parse();

            var collectionName = pipeline.CollectionName;
            var pipelineDocuments = GetPipelineDocuments(pipeline.Stages);

            var operation = new AggregateOperation<BsonDocument>(
                new CollectionNamespace(new DatabaseNamespace(_connection.Database), collectionName),
                pipelineDocuments,
                BsonDocumentSerializer.Instance,
                new MessageEncoderSettings());

            var result = _connection.ExecuteOperation(operation);
            return new MongoDataReader(result);
        }

        private IEnumerable<BsonDocument> GetPipelineDocuments(IEnumerable<PipelineStage> stages)
        {
            foreach (var stage in stages)
            {
                if (stage.PipelineOperator == "MATCH")
                {
                    yield return new BsonDocument("$match", BsonDocument.Parse(stage.Document));
                }
                else if (stage.PipelineOperator == "PROJECT")
                {
                    yield return new BsonDocument("$project", BsonDocument.Parse(stage.Document));
                }
            }
        }
    }
}
