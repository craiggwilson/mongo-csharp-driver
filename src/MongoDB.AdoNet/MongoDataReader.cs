using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.AdoNet
{
    public class MongoDataReader : DbDataReader
    {
        private readonly IAsyncCursor<BsonDocument> _cursor;
        private IEnumerator<BsonDocument> _currentEnumerator;
        private BsonDocumentDataRecord _currentDataRecord;
        private bool _isClosed;

        internal MongoDataReader(IAsyncCursor<BsonDocument> cursor)
        {
            _cursor = cursor;
        }

        public override object this[string name]
        {
            get { return GetCurrentDataRecord()[name]; }
        }

        public override object this[int ordinal]
        {
            get { return GetCurrentDataRecord()[ordinal]; }
        }

        public override int Depth
        {
            get { return 0; }
        }

        public override int FieldCount
        {
            get { return GetCurrentDataRecord().FieldCount; }
        }

        public override bool HasRows
        {
            get { return false; }
        }

        public override bool IsClosed
        {
            get { return _isClosed; }
        }

        public override int RecordsAffected
        {
            get { return -1; }
        }

        public override bool GetBoolean(int ordinal)
        {
            return GetCurrentDataRecord().GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return GetCurrentDataRecord().GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return GetCurrentDataRecord().GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            return GetCurrentDataRecord().GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return GetCurrentDataRecord().GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return GetCurrentDataRecord().GetDataTypeName(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return GetCurrentDataRecord().GetDateTime(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return GetCurrentDataRecord().GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return GetCurrentDataRecord().GetDouble(ordinal);
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this);
        }

        public override Type GetFieldType(int ordinal)
        {
            return GetCurrentDataRecord().GetFieldType(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return GetCurrentDataRecord().GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return GetCurrentDataRecord().GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return GetCurrentDataRecord().GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return GetCurrentDataRecord().GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return GetCurrentDataRecord().GetInt64(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return GetCurrentDataRecord().GetName(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            return GetCurrentDataRecord().GetOrdinal(name);
        }

        public override string GetString(int ordinal)
        {
            return GetCurrentDataRecord().GetString(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            return GetCurrentDataRecord().GetValue(ordinal);
        }

        public override int GetValues(object[] values)
        {
            return GetCurrentDataRecord().GetValues(values);
        }

        public override bool IsDBNull(int ordinal)
        {
            return GetCurrentDataRecord().IsDBNull(ordinal);
        }

        public override bool NextResult()
        {
            _isClosed = true;
            return false;
        }

        public override bool Read()
        {
            return ReadAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            while (_currentEnumerator == null || !_currentEnumerator.MoveNext())
            {
                if (!await _cursor.MoveNextAsync(cancellationToken))
                {
                    _isClosed = true;
                    return false;
                }
                _currentEnumerator = _cursor.Current.GetEnumerator();
            }

            _currentDataRecord = new BsonDocumentDataRecord(_currentEnumerator.Current);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cursor.Dispose();
            }
            base.Dispose(disposing);
        }

        private BsonDocumentDataRecord GetCurrentDataRecord()
        {
            if (_isClosed)
            {
                throw new InvalidOperationException("Data reader is closed.");
            }
            if(_currentDataRecord == null)
            {
                throw new InvalidOperationException("Must call Read on data reader first.");
            }

            return _currentDataRecord;
        }
    }
}
