/* Copyright 2010-2015 MongoDB Inc.
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
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Relinq.Structure.Expressions
{
    internal class ArrayItemExpression : SerializationExpression
    {
        private readonly IFieldExpression _referencedField;
        private readonly IBsonSerializer _serializer;

        public ArrayItemExpression(IFieldExpression referencedField, IBsonSerializer serializer)
        {
            _referencedField = Ensure.IsNotNull(referencedField, "field");
            _serializer = Ensure.IsNotNull(serializer, "serializer");
        }

        public override MongoExpressionType MongoNodeType
        {
            get { return MongoExpressionType.ArrayItem; }
        }

        public override ExpressionType NodeType
        {
            get { return ExpressionType.Extension; }
        }

        public IFieldExpression ReferencedField
        {
            get { return _referencedField; }
        }

        public override IBsonSerializer Serializer
        {
            get { return _serializer; }
        }

        public override Type Type
        {
            get { return _serializer.ValueType; }
        }

        public override string ToString()
        {
            return "[document]";
        }

        protected override Expression Accept(MongoExpressionVisitor visitor)
        {
            return visitor.VisitArrayItem(this);
        }
    }
}