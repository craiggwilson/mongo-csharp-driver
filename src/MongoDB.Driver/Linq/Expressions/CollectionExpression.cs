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
        public CollectionExpression(Type documentType, Expression queryable)
            : base(LinqToMongoExpressionType.Collection, queryable.Type)
        {
            _documentType = documentType;
            _queryable = queryable;
        }

        // public properties
        public Type DocumentType
        {
            get { return _documentType; }
        }

        public Expression Queryable
        {
            get { return _queryable; }
        }

        // public methods
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