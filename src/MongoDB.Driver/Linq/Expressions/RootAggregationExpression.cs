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

namespace MongoDB.Driver.Linq.Expressions
{
    [DebuggerTypeProxy(typeof(RootAggregationExpressionDebugView))]
    [DebuggerDisplay("{ToString()}")]
    internal class RootAggregationExpression : LinqToMongoExpression
    {
        // private fields
        private readonly RootAggregationType _aggregationType;
        private readonly Expression _projector;
        private readonly Expression _source;

        // constructors
        public RootAggregationExpression(Expression source, Expression projector, RootAggregationType aggregationType)
            : base(LinqToMongoExpressionType.RootAggregation, projector.Type)
        {
            _source = source;
            _projector = projector;
            _aggregationType = aggregationType;
        }

        // public properties
        public RootAggregationType AggregationType
        {
            get { return _aggregationType; }
        }

        public Expression Projector
        {
            get { return _projector; }
        }

        public Expression Source
        {
            get { return _source; }
        }

        // public methods
        public override string ToString()
        {
            return LinqToMongoExpressionFormatter.ToString(this);
        }
    }

    internal class RootAggregationExpressionDebugView
    {
        private readonly RootAggregationExpression _node;

        public RootAggregationExpressionDebugView(RootAggregationExpression node)
        {
            _node = node;
        }

        public RootAggregationType AggregationType
        {
            get { return _node.AggregationType; }
        }

        public Expression Projector
        {
            get { return _node.Projector; }
        }

        public Expression Source
        {
            get { return _node.Source; }
        }

        public Type Type
        {
            get { return _node.Type; }
        }
    }
}