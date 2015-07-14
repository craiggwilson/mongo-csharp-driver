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
* 
*/

using System;
using System.Linq.Expressions;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Relinq.Structure.Expressions
{
    internal class FilterExpression : MongoExpression
    {
        private readonly string _itemName;
        private readonly Expression _predicate;
        private readonly Expression _source;

        public FilterExpression(Expression source, string itemName, Expression predicate)
        {
            _source = Ensure.IsNotNull(source, "source");
            _itemName = Ensure.IsNotNull(itemName, "itemName");
            _predicate = Ensure.IsNotNull(predicate, "predicate");
        }

        public string ItemName
        {
            get { return _itemName; }
        }

        public Expression Predicate
        {
            get { return _predicate; }
        }

        public Expression Source
        {
            get { return _source; }
        }

        public override MongoExpressionType MongoNodeType
        {
            get { return MongoExpressionType.Filter; }
        }

        public override Type Type
        {
            get { return _source.Type; }
        }

        public Expression Update(Expression source, Expression predicate)
        {
            if (source != _source || predicate != _predicate)
            {
                return new FilterExpression(source, _itemName, predicate);
            }

            return this;
        }

        protected override Expression Accept(MongoExpressionVisitor visitor)
        {
            return visitor.VisitFilter(this);
        }
    }
}
