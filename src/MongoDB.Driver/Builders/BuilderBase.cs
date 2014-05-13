/* Copyright 2010-2014 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.Builders
{
    /// <summary>
    /// Abstract base class for the builders.
    /// </summary>
    [Serializable]
    [BsonSerializer(typeof(BuilderBase.Serializer))]
    public abstract class BuilderBase : IConvertibleToBsonDocument
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the BuilderBase class.
        /// </summary>
        protected BuilderBase()
        {
        }

        // public methods
        /// <summary>
        /// Returns the result of the builder as a BsonDocument.
        /// </summary>
        /// <returns>A BsonDocument.</returns>
        public abstract BsonDocument ToBsonDocument();

        /// <summary>
        /// Returns a string representation of the settings.
        /// </summary>
        /// <returns>A string representation of the settings.</returns>
        public override string ToString()
        {
            return this.ToJson(); // "this." required to access extension method
        }

        // explicit interface implementations
        BsonDocument IConvertibleToBsonDocument.ToBsonDocument()
        {
            return ToBsonDocument();
        }

        // nested classes
        internal class Serializer : UndiscriminatedActualTypeSerializer<BuilderBase>
        {
	}
    }

    /// <summary>
    /// Abstract base class for Typed builders.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public abstract class BuilderBase<TDocument> : BuilderBase
    {
        // private fields
        private readonly BuilderHelper _helper;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderBase{TDocument}" /> class.
        /// </summary>
        protected BuilderBase()
        {
            _helper = new BuilderHelper(typeof(TDocument));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderBase{TDocument}" /> class.
        /// </summary>
        /// <param name="binder">The binder.</param>
        internal BuilderBase(SerializationInfoBinder binder)
        {
            _helper = new BuilderHelper(binder);
        }

        // protected methods
        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        /// <param name="memberExpression">The member expression.</param>
        /// <returns></returns>
        protected string GetElementName(LambdaExpression memberExpression)
        {
            return _helper.GetElementName( memberExpression);
        }

        /// <summary>
        /// Gets the element names.
        /// </summary>
        /// <param name="memberExpressions">The member expressions.</param>
        /// <returns></returns>
        protected IEnumerable<string> GetElementNames(IEnumerable<LambdaExpression> memberExpressions)
        {
            return _helper.GetElementNames(memberExpressions);
        }

        /// <summary>
        /// Gets the serialization info.
        /// </summary>
        /// <param name="memberExpression">The member expression.</param>
        /// <returns></returns>
        protected BsonSerializationInfo GetSerializationInfo(LambdaExpression memberExpression)
        {
            return _helper.GetSerializationInfo(memberExpression);
        }

        /// <summary>
        /// Gets the item serialization info.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException"></exception>
        protected BsonSerializationInfo GetItemSerializationInfo(string methodName, BsonSerializationInfo serializationInfo)
        {
            return _helper.GetItemSerializationInfo(methodName, serializationInfo);
        }
    }
}
