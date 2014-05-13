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
    /// An collection expression.
    /// </summary>
    /// <remarks>
    /// This will almost always be the deepest expression in the tree.
    /// </remarks>
    [DebuggerTypeProxy(typeof(CollectionExpressionDebugView))]
    [DebuggerDisplay("{ToString()}")]
    internal class CollectionExpression : LinqToMongoExpression
    {
        // private fields
        private readonly Type _documentType;
        private readonly Expression _queryable;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionExpression" /> class.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="queryable">The queryable.</param>
        public CollectionExpression(Type documentType, Expression queryable)
            : base(LinqToMongoExpressionType.Collection, queryable.Type)
        {
            _documentType = documentType;
            _queryable = queryable;
        }

        // public properties
        /// <summary>
        /// Gets the type of the document.
        /// </summary>
        public Type DocumentType
        {
            get { return _documentType; }
        }

        /// <summary>
        /// Gets the queryable.
        /// </summary>
        public Expression Queryable
        {
            get { return _queryable; }
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

    internal class CollectionExpressionDebugView
    {
        private readonly CollectionExpression _node;

        public CollectionExpressionDebugView(CollectionExpression node)
        {
            _node = node;
        }

        public Type DocumentType
        {
            get { return _node.DocumentType; }
        }
    }
}