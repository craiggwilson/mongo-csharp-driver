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
    /// A skip and limit expression.
    /// </summary>
    [DebuggerTypeProxy(typeof(SkipLimitExpressionDebugView))]
    [DebuggerDisplay("{ToString()}")]
    internal class SkipLimitExpression : LinqToMongoExpression
    {
        // private fields
        private readonly Expression _limit;
        private readonly Expression _skip;
        private readonly Expression _source;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SkipLimitExpression" /> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        public SkipLimitExpression(Expression source, Expression skip, Expression limit)
            : base(LinqToMongoExpressionType.SkipLimit, source.Type)
        {
            _limit = limit;
            _skip = skip;
            _source = source;
        }

        // public properties
        /// <summary>
        /// Gets the limit.
        /// </summary>
        public Expression Limit
        {
            get { return _limit; }
        }

        /// <summary>
        /// Gets the skip.
        /// </summary>
        public Expression Skip
        {
            get { return _skip; }
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

    internal class SkipLimitExpressionDebugView
    {
        private readonly SkipLimitExpression _node;

        public SkipLimitExpressionDebugView(SkipLimitExpression node)
        {
            _node = node;
        }

        public Expression Limit
        {
            get { return _node.Limit; }
        }

        public Expression Skip
        {
            get { return _node.Skip; }
        }

        public Expression Source
        {
            get { return _node.Source; }
        }
    }
}