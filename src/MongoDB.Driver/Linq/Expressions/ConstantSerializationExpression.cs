/* Copyright 2015 MongoDB Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Expressions
{
    internal sealed class ConstantSerializationExpression : SerializationExpression
    {
        private readonly IBsonSerializer _serializer;
        private readonly Type _type;
        private readonly object _value;

        public ConstantSerializationExpression(object value, Type type, IBsonSerializer serializer)
        {
            _value = Ensure.IsNotNull(value, nameof(value)); ;
            _type = Ensure.IsNotNull(type, nameof(type));
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
        }

        public override ExtensionExpressionType ExtensionType => ExtensionExpressionType.Constant;

        public override IBsonSerializer Serializer => _serializer;

        public override Type Type => _type;

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitConstantSerialization(this);
        }
    }
}
