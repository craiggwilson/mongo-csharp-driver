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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

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
        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationExpression" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="aggregationType">Type of the aggregation.</param>
        /// <param name="argument">The argument.</param>
        public AggregationExpression(Type type, AggregationType aggregationType, Expression argument)
            : base(LinqToMongoExpressionType.Aggregation, type)
        {
            _aggregationType = aggregationType;
            _argument = argument;
        }

        // public properties
        /// <summary>
        /// Gets the type of the aggregation.
        /// </summary>
        public AggregationType AggregationType
        {
            get { return _aggregationType; }
        }

        /// <summary>
        /// Gets the argument.
        /// </summary>
        public Expression Argument
        {
            get { return _argument; }
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