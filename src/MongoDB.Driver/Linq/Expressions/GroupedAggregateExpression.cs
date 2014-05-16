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

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// A temporary expressino used to correlated an AggregationExpression with a GroupExpression.
    /// </summary>
    [DebuggerTypeProxy(typeof(GroupExpressionDebugView))]
    [DebuggerDisplay("{ToString()}")]
    internal class GroupedAggregateExpression : LinqToMongoExpression
    {
        // private fields
        private readonly Guid _groupCorrelationId;
        private readonly AggregationExpression _aggregation;

        // constructors
        public GroupedAggregateExpression(Guid groupCorrelationId, AggregationExpression aggregation)
            : base(LinqToMongoExpressionType.GroupedAggregate, aggregation.Type)
        {
            _groupCorrelationId = groupCorrelationId;
            _aggregation = aggregation;
        }

        // public properties
        public AggregationExpression Aggregation
        {
            get { return _aggregation; }
        }

        public Guid GroupCorrelationId
        {
            get { return _groupCorrelationId; }
        }

        // public methods
        public override string ToString()
        {
            return LinqToMongoExpressionFormatter.ToString(this);
        }
    }

    internal class GroupedAggregateExpressionDebugView
    {
        private readonly GroupedAggregateExpression _node;

        public GroupedAggregateExpressionDebugView(GroupedAggregateExpression node)
        {
            _node = node;
        }

        public AggregationExpression Aggregation
        {
            get { return _node.Aggregation; }
        }

        public Type Type
        {
            get { return _node.Type; }
        }
    }
}