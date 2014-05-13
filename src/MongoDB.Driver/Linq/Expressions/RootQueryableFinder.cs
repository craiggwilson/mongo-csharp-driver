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
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// Finds the root queryable in a node.
    /// </summary>
    /// <remarks>
    /// This will almost always be the deepest node in the tree.
    /// </remarks>
    internal class RootQueryableFinder : LinqToMongoExpressionVisitor
    {
        // private fields
        private ConstantExpression _root;

        // public methods
        /// <summary>
        /// Finds the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The root queryable or null.</returns>
        public ConstantExpression Find(Expression node)
        {
            _root = null;
            Visit(node);
            return _root;
        }

        // protected methods
        /// <summary>
        /// Visits a ConstantExpression.
        /// </summary>
        /// <param name="node">The ConstantExpression.</param>
        /// <returns>
        /// The ConstantExpression (possibly modified).
        /// </returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (_root == null && node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(LinqToMongoQueryable<>))
            {
                _root = node;
            }

            return node;
        }
    }
}