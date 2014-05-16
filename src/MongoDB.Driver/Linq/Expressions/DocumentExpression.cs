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

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <remarks>
    /// This will generally be in the place of a ParameterExpression. Hence, the Expression
    /// property will almost always be a ParameterExpression.
    /// </remarks>
    [DebuggerTypeProxy(typeof(DocumentExpressionDebugView))]
    [DebuggerDisplay("{ToString()}")]
    internal class DocumentExpression : LinqToMongoExpression, IBsonSerializationInfoExpression
    {
        // private fields
        private readonly Expression _expression;
        private readonly bool _isProjected;
        private readonly BsonSerializationInfo _serializationInfo;

        // constructors
        public DocumentExpression(Expression expression, BsonSerializationInfo serializationInfo, bool isProjected)
            : base(LinqToMongoExpressionType.Document, expression.Type)
        {
            _expression = expression;
            _serializationInfo = serializationInfo;
            _isProjected = isProjected;
        }

        // public properties
        public Expression Expression
        {
            get { return _expression; }
        }

        public bool IsProjected
        {
            get { return _isProjected; }
        }

        public BsonSerializationInfo SerializationInfo
        {
            get { return _serializationInfo; }
        }

        // public methods
        public override string ToString()
        {
            return LinqToMongoExpressionFormatter.ToString(this);
        }
    }

    internal class DocumentExpressionDebugView
    {
        private readonly DocumentExpression _node;

        public DocumentExpressionDebugView(DocumentExpression node)
        {
            _node = node;
        }

        public Expression Expression
        {
            get { return _node.Expression; }
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