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
    /// <summary>
    /// An aggregate expression (min,max,first,sum,avg,etc...).
    /// </summary>
    [DebuggerTypeProxy(typeof(AggregateExpressionDebugView))]
    [DebuggerDisplay("{ToString()}")]
    internal class AggregationExpression : LinqToMongoExpression
    {
        // private fields
        private readonly AggregationType _aggregationType;
        private readonly Expression _argument;

        //constructors
        public AggregationExpression(Type type, AggregationType aggregationType, Expression argument)
            : base(LinqToMongoExpressionType.Aggregation, type)
        {
            _aggregationType = aggregationType;
            _argument = argument;
        }

        // public properties
        public AggregationType AggregationType
        {
            get { return _aggregationType; }
        }

        public Expression Argument
        {
            get { return _argument; }
        }

        // public methods
        public override string ToString()
        {
            return LinqToMongoExpressionFormatter.ToString(this);
        }
    }

    internal class AggregateExpressionDebugView
    {
        private readonly AggregationExpression _node;

        public AggregateExpressionDebugView(AggregationExpression node)
        {
            _node = node;
        }

        public AggregationType AggregationType
        {
            get { return _node.AggregationType; }
        }

        public Expression Argument
        {
            get { return _node.Argument; }
        }

        public Type Type
        {
            get { return _node.Type; }
        }
    }
}