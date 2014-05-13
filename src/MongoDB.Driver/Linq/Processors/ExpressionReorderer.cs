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

using MongoDB.Driver.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MongoDB.Driver.Linq.Processors
{
    /// <summary>
    /// Reorders expressions to allow for further optimization.
    /// </summary>
    /// <remarks>
    /// Specifically, this will reorder MatchExpressions and SortExpressions
    /// such that the MatchExpression will always be before(deeper than) the 
    /// SortExpression.
    /// </remarks>
    internal class ExpressionReorderer : LinqToMongoExpressionVisitor
    {
        // public methods
        /// <summary>
        /// Reorders the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public Expression Reorder(Expression node)
        {
            return Visit(node);
        }

        // protected methods
        /// <summary>
        /// Visits the match.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        /// The MatchExpression (possibly modified).
        /// </returns>
        protected override Expression VisitMatch(MatchExpression node)
        {
            var source = Visit(node.Source);

            var sourceAsSort = source as SortExpression;
            if (sourceAsSort != null)
            {
                var newMatch = new MatchExpression(sourceAsSort.Source, node.Match);
                return new SortExpression(newMatch, sourceAsSort.SortClauses);
            }

            if (node.Source != null)
            {
                node = new MatchExpression(source, node.Match);
            }

            return node;
        }
    }
}