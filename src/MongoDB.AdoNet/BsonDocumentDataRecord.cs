using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.AdoNet
{
    public class BsonDocumentDataRecord : DbDataRecord
    {
        private readonly BsonDocument _document;

        internal BsonDocumentDataRecord(BsonDocument document)
        {
            _document = document;
        }

        public override object this[string name]
        {
            get { return _document[name]; }
        }

        public override object this[int i]
        {
            get { return _document[i]; }
        }

        public override int FieldCount
        {
            get { return _document.ElementCount; }
        }

        public override bool GetBoolean(int i)
        {
            return _document[i].ToBoolean();
        }

        public override byte GetByte(int i)
        {
            return ((BsonBinaryData)_document[i]).Bytes.Single();
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            var bytes = ((BsonBinaryData)_document[i]).Bytes;
            Buffer.BlockCopy(bytes, (int)dataIndex, buffer, bufferIndex, length);
            return length; //TODO: this isn't right
        }

        public override char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            throw new NotImplementedException();
        }

        public override string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int i)
        {
            return (DateTime)_document[i];
        }

        public override decimal GetDecimal(int i)
        {
            return (decimal)_document[i].ToDouble();
        }

        public override double GetDouble(int i)
        {
            return _document[i].ToDouble();
        }

        public override Type GetFieldType(int i)
        {
            return _document[i].GetType();
        }

        public override float GetFloat(int i)
        {
            return (float)_document[i].ToDouble();
        }

        public override Guid GetGuid(int i)
        {
            return ((BsonBinaryData)_document[i]).ToGuid();
        }

        public override short GetInt16(int i)
        {
            return (short)_document[i].ToInt32();
        }

        public override int GetInt32(int i)
        {
            return _document[i].ToInt32();
        }

        public override long GetInt64(int i)
        {
            return _document[i].ToInt64();
        }

        public override string GetName(int i)
        {
            return _document.GetElement(i).Name;
        }

        public override int GetOrdinal(string name)
        {
            return _document.IndexOfName(name);
        }

        public override string GetString(int i)
        {
            return _document[i].ToString();
        }

        public override object GetValue(int i)
        {
            return _document[i];
        }

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public override bool IsDBNull(int i)
        {
            if(i < 0)
            {
                return true;
            }

            return _document[i].IsBsonNull;
        }
    }
}
