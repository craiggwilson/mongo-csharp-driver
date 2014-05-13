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
    /// A match expression.
    /// </summary>
    [DebuggerTypeProxy(typeof(MatchExpressionDebugView))]
    [DebuggerDisplay("{ToString()}")]
    internal class MatchExpression : LinqToMongoExpression
    {
        // private fields
        private readonly Expression _match;
        private readonly Expression _source;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MatchExpression" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="match">The match.</param>
        public MatchExpression(Expression source, Expression match)
            : base(LinqToMongoExpressionType.Match, source.Type)
        {
            _match = match;
            _source = source;
        }

        // public properties
        /// <summary>
        /// Gets the match.
        /// </summary>
        public Expression Match
        {
            get { return _match; }
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        public Expression Source
        {
            get { return _source; }
        }
        
        // public methods
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

    internal class MatchExpressionDebugView
    {
        private readonly MatchExpression _node;

        public MatchExpressionDebugView(MatchExpression node)
        {
            _node = node;
        }

        public Expression Match
        {
            get { return _node.Match; }
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