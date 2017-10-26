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

using System;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// An abstract base class for an IBsonWriter that wraps another IBsonWriter.
    /// </summary>
    /// <seealso cref="MongoDB.Bson.IO.IBsonWriter" />
    public abstract class WrappingBsonWriter : IBsonWriter
    {
        // private fields
        private readonly IBsonWriter _wrapped;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WrappingBsonWriter"/> class.
        /// </summary>
        /// <param name="wrapped">The wrapped writer.</param>
        public WrappingBsonWriter(IBsonWriter wrapped)
        {
            _wrapped = wrapped ?? throw new ArgumentNullException(nameof(wrapped));
        }

        // public properties
        /// <inheritdoc />
        public virtual long Position => _wrapped.Position;

        /// <inheritdoc />
        public virtual int SerializationDepth => _wrapped.SerializationDepth;

        /// <inheritdoc />
        public virtual BsonWriterSettings Settings => _wrapped.Settings;

        /// <inheritdoc />
        public virtual BsonWriterState State => _wrapped.State;

        /// <summary>
        /// Gets the wrapped writer.
        /// </summary>
        /// <value>
        /// The wrapped writer.
        /// </value>
        public IBsonWriter Wrapped => _wrapped;

        // public methods
        /// <inheritdoc />
        public virtual void Close()
        {
            _wrapped.Close();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        public virtual void Flush()
        {
            _wrapped.Flush();
        }

        /// <inheritdoc />
        public virtual void PopElementNameValidator()
        {
            _wrapped.PopElementNameValidator();
        }

        /// <inheritdoc />
        public virtual void PopSettings()
        {
            _wrapped.PopSettings();
        }

        /// <inheritdoc />
        public virtual void PushElementNameValidator(IElementNameValidator validator)
        {
            _wrapped.PushElementNameValidator(validator);
        }

        /// <inheritdoc />
        public virtual void PushSettings(Action<BsonWriterSettings> configurator)
        {
            _wrapped.PushSettings(configurator);
        }

        /// <inheritdoc />
        public virtual void WriteBinaryData(BsonBinaryData binaryData)
        {
            _wrapped.WriteBinaryData(binaryData);
        }

        /// <inheritdoc />
        public virtual void WriteBoolean(bool value)
        {
            _wrapped.WriteBoolean(value);
        }

        /// <inheritdoc />
        public virtual void WriteBytes(byte[] bytes)
        {
            _wrapped.WriteBytes(bytes);
        }

        /// <inheritdoc />
        public virtual void WriteDateTime(long value)
        {
            _wrapped.WriteDateTime(value);
        }

        /// <inheritdoc />
        public virtual void WriteDecimal128(Decimal128 value)
        {
            _wrapped.WriteDecimal128(value);
        }

        /// <inheritdoc />
        public virtual void WriteDouble(double value)
        {
            _wrapped.WriteDouble(value);
        }

        /// <inheritdoc />
        public virtual void WriteEndArray()
        {
            _wrapped.WriteEndArray();
        }

        /// <inheritdoc />
        public virtual void WriteEndDocument()
        {
            _wrapped.WriteEndDocument();
        }

        /// <inheritdoc />
        public virtual void WriteInt32(int value)
        {
            _wrapped.WriteInt32(value);
        }

        /// <inheritdoc />
        public virtual void WriteInt64(long value)
        {
            _wrapped.WriteInt64(value);
        }

        /// <inheritdoc />
        public virtual void WriteJavaScript(string code)
        {
            _wrapped.WriteJavaScript(code);
        }

        /// <inheritdoc />
        public virtual void WriteJavaScriptWithScope(string code)
        {
            _wrapped.WriteJavaScriptWithScope(code);
        }

        /// <inheritdoc />
        public virtual void WriteMaxKey()
        {
            _wrapped.WriteMaxKey();
        }

        /// <inheritdoc />
        public virtual void WriteMinKey()
        {
            _wrapped.WriteMinKey();
        }

        /// <inheritdoc />
        public virtual void WriteName(string name)
        {
            _wrapped.WriteName(name);
        }

        /// <inheritdoc />
        public virtual void WriteNull()
        {
            _wrapped.WriteNull();
        }

        /// <inheritdoc />
        public virtual void WriteObjectId(ObjectId objectId)
        {
            _wrapped.WriteObjectId(objectId);
        }

        /// <inheritdoc />
        public virtual void WriteRawBsonArray(IByteBuffer slice)
        {
            _wrapped.WriteRawBsonArray(slice);
        }

        /// <inheritdoc />
        public virtual void WriteRawBsonDocument(IByteBuffer slice)
        {
            _wrapped.WriteRawBsonDocument(slice);
        }

        /// <inheritdoc />
        public virtual void WriteRegularExpression(BsonRegularExpression regex)
        {
            _wrapped.WriteRegularExpression(regex);
        }

        /// <inheritdoc />
        public virtual void WriteStartArray()
        {
            _wrapped.WriteStartArray();
        }

        /// <inheritdoc />
        public virtual void WriteStartDocument()
        {
            _wrapped.WriteStartDocument();
        }

        /// <inheritdoc />
        public virtual void WriteString(string value)
        {
            _wrapped.WriteString(value);
        }

        /// <inheritdoc />
        public virtual void WriteSymbol(string value)
        {
            _wrapped.WriteSymbol(value);
        }

        /// <inheritdoc />
        public virtual void WriteTimestamp(long value)
        {
            _wrapped.WriteTimestamp(value);
        }

        /// <inheritdoc />
        public virtual void WriteUndefined()
        {
            _wrapped.WriteUndefined();
        }

        // protected methods
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _wrapped.Dispose();
            }
        }
    }
}
