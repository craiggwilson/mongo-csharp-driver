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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MongoDB.Driver.Linq.Processors
{
    /// <summary>
    /// Reduces adjacent SortExpressions.
    /// </summary>
    internal class SortReducer : LinqToMongoExpressionVisitor
    {
        // public methods
        /// <summary>
        /// Reduces the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public Expression Reduce(Expression node)
        {
            return Visit(node);
        }

        // protected methods
        /// <summary>
        /// Visits the sort.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The SortExpression (possibly modified).</returns>
        protected override Expression VisitSort(SortExpression node)
        {
            var source = Visit(node.Source);

            // If there are 2 sort expressions right next to each other
            // then we could possibly just ignore the first one. However, if a stable
            // sort is run on the server, then we should preserve the original
            // order as well, so we simply append the earlier orderings to the
            // later ones.

            var sourceAsSort = source as SortExpression;
            if (sourceAsSort != null)
            {
                var orderings = node.SortClauses.ToList();
                orderings.AddRange(sourceAsSort.SortClauses);

                return new SortExpression(sourceAsSort.Source, RemoveRedundantSortClauses(orderings));
            }

            if (node.Source != source)
            {
                node = new SortExpression(source, node.SortClauses);
            }

            return node;
        }

        // private methods
        private ReadOnlyCollection<SortClause> RemoveRedundantSortClauses(IEnumerable<SortClause> sortClauses)
        {
            List<SortClause> newSortClauses = new List<SortClause>();
            var existingFields = new HashSet<Expression>(new OrderingExpressionEqualityComparer());
            foreach (var sortClause in sortClauses)
            {
                if (!existingFields.Contains(sortClause.Expression))
                {
                    existingFields.Add(sortClause.Expression);
                    newSortClauses.Add(sortClause);
                }
            }

            return newSortClauses.AsReadOnly();
        }

        private class OrderingExpressionEqualityComparer : IEqualityComparer<Expression>
        {
            public bool Equals(Expression x, Expression y)
            {
                if (x == y)
                {
                    return true;
                }

                var xField = x as FieldExpression;
                var yField = y as FieldExpression;
                if (xField != null && yField != null)
                {
                    return xField.SerializationInfo.ElementName == yField.SerializationInfo.ElementName;
                }

                return false;
            }

            public int GetHashCode(Expression obj)
            {
                // make all expressions of the same type 
                // return the same hash code so the Equals method
                // gets called for all expressions that are possibly the same.
                return obj.GetType().GetHashCode();
            }
        }

        
    }
}