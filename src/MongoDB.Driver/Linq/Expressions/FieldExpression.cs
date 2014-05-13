/* Copyright 2010-2012 10gen Inc.
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

using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// A field expression.
    /// </summary>
    /// <remarks>
    /// Will take the place of a number of different types of expressions.  Any 
    /// type of Expression that is possible to be used to identify a member or 
    /// array element. 
    /// </remarks>
    [DebuggerTypeProxy(typeof(FieldExpressionDebugView))]
    [DebuggerDisplay("{ToString()}")]
    internal class FieldExpression : LinqToMongoExpression, IBsonSerializationInfoExpression
    {
        // private fields
        private readonly Expression _expression;
        private readonly bool _isProjected;
        private readonly BsonSerializationInfo _serializationInfo;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldExpression" /> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="serializationInfo">The serialization info.</param>
        /// <param name="isProjected">if set to <c>true</c> the the field is the result of a projection.</param>
        public FieldExpression(Expression expression, BsonSerializationInfo serializationInfo, bool isProjected)
            : base(LinqToMongoExpressionType.Field, expression.Type)
        {
            _expression = expression;
            _serializationInfo = serializationInfo;
            _isProjected = isProjected;
        }

        // public properties
        /// <summary>
        /// Gets the original expression.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
        }

        /// <summary>
        /// Gets a value indicating whether this serialization info is the result of a projection.
        /// </summary>
        public bool IsProjected
        {
            get { return _isProjected; }
        }

        /// <summary>
        /// Gets the serialization info.
        /// </summary>
        public BsonSerializationInfo SerializationInfo
        {
            get { return _serializationInfo; }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return LinqToMongoExpressionFormatter.ToString(this);
        }
    }

    internal class FieldExpressionDebugView
    {
        private readonly FieldExpression _node;

        public FieldExpressionDebugView(FieldExpression node)
        {
            _node = node;
        }

        public string ElementName
        {
            get { return _node.SerializationInfo == null ? "(null)" : _node.SerializationInfo.ElementName; }
        }

        public Expression Expression
        {
            get { return _node.Expression; }
        }

        public bool IsProjected
        {
            get { return _node.IsProjected; }
        }

        public Type SerializerType
        {
            get { return _node.SerializationInfo == null ? null : _node.SerializationInfo.Serializer.GetType(); }
        }

        public Type Type
        {
            get { return _node.Type; }
        }
    }
}