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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// A visitor that includes support for the custom AggExpression types.
    /// </summary>
    internal class LinqToMongoExpressionVisitor : ExpressionVisitor
    {
        /// <summary>
        /// Gets the lambda expression.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        protected LambdaExpression GetLambda(Expression node)
        {
            return (LambdaExpression)StripQuotes(node);
        }

        /// <summary>
        /// Determines whether the specified node is a lambda expression.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns><c>true</c> if the node is a lambda expression; otherwise <c>false</c>.</returns>
        protected bool IsLambda(Expression node)
        {
            return StripQuotes(node).NodeType == ExpressionType.Lambda;
        }

        /// <summary>
        /// Determines whether the specified node is a lambda expression with the specified number of parameters.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="parameterCount">The parameter count.</param>
        /// <returns><c>true</c> if the node is a lambda expression; otherwise <c>false</c>.</returns>
        protected bool IsLambda(Expression node, int parameterCount)
        {
            var lambda = StripQuotes(node);
            return lambda.NodeType == ExpressionType.Lambda &&
                ((LambdaExpression)lambda).Parameters.Count == parameterCount;
        }

        /// <summary>
        /// Indicates whether the node is a linq method.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="names">The names.</param>
        /// <returns><c>true</c> if [is linq method] [the specified node]; otherwise, <c>false</c>.</returns>
        protected bool IsLinqMethod(MethodCallExpression node, params string[] names)
        {
            if (node.Method.DeclaringType != typeof(Enumerable) && node.Method.DeclaringType != typeof(Queryable))
            {
                return false;
            }

            if (names == null || names.Length == 0)
            {
                return true;
            }

            return names.Contains(node.Method.Name);
        }

        /// <summary>
        /// Strips the quotes from an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>An unquoted expression.</returns>
        protected Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

        /// <summary>
        /// Visits an Expression.
        /// </summary>
        /// <param name="node">The Expression.</param>
        /// <returns>The Expression (possibly modified).</returns>
        protected override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }
            switch ((LinqToMongoExpressionType)node.NodeType)
            {
                case LinqToMongoExpressionType.Aggregation:
                    return VisitAggregation((AggregationExpression)node);
                case LinqToMongoExpressionType.Collection:
                    return VisitCollection((CollectionExpression)node);
                case LinqToMongoExpressionType.Distinct:
                    return VisitDistinct((DistinctExpression)node);
                case LinqToMongoExpressionType.Document:
                    return VisitDocument((DocumentExpression)node);
                case LinqToMongoExpressionType.Field:
                    return VisitField((FieldExpression)node);
                case LinqToMongoExpressionType.Group:
                    return VisitGroup((GroupExpression)node);
                case LinqToMongoExpressionType.GroupedAggregate:
                    return VisitGroupedAggregate((GroupedAggregateExpression)node);
                case LinqToMongoExpressionType.Match:
                    return VisitMatch((MatchExpression)node);
                case LinqToMongoExpressionType.Pipeline:
                    return VisitPipeline((PipelineExpression)node);
                case LinqToMongoExpressionType.Project:
                    return VisitProject((ProjectExpression)node);
                case LinqToMongoExpressionType.RootAggregation:
                    return VisitRootAggregation((RootAggregationExpression)node);
                case LinqToMongoExpressionType.SkipLimit:
                    return VisitSkipLimit((SkipLimitExpression)node);
                case LinqToMongoExpressionType.Sort:
                    return VisitSort((SortExpression)node);
                default:
                    return base.Visit(node);
            }
        }

        /// <summary>
        /// Visits the aggregate.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The AggregationExpression (possibly modified).</returns>
        protected virtual Expression VisitAggregation(AggregationExpression node)
        {
            var argument = Visit(node.Argument);
            if (node.Argument != argument)
            {
                node = new AggregationExpression(node.Type, node.AggregationType, argument);
            }

            return node;
        }

        /// <summary>
        /// Visits the collection.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The CollectionExpression (possibly modified).</returns>
        protected virtual Expression VisitCollection(CollectionExpression node)
        {
            var queryable = Visit(node.Queryable);
            if (node.Queryable != queryable)
            {
                node = new CollectionExpression(node.DocumentType, queryable);
            }

            return node;
        }

        /// <summary>
        /// Visits the distinct.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The DistinctExpression (possibly modified).</returns>
        protected virtual Expression VisitDistinct(DistinctExpression node)
        {
            var source = Visit(node.Source);
            var projector = Visit(node.Projector);

            if (node.Source != source ||
                node.Projector != projector)
            {
                node = new DistinctExpression(source, projector);
            }

            return node;
        }

        /// <summary>
        /// Visits the document.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The DocumentExpression (possibly modified).</returns>
        protected virtual Expression VisitDocument(DocumentExpression node)
        {
            return node;
        }

        /// <summary>
        /// Visits the field.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The FieldExpression (possibly modified).</returns>
        protected virtual Expression VisitField(FieldExpression node)
        {
            return node;
        }

        /// <summary>
        /// Visits the group.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The GroupExpression (possibly modified).</returns>
        protected virtual Expression VisitGroup(GroupExpression node)
        {
            var source = Visit(node.Source);
            var id = Visit(node.Id);
            var aggregations = Visit(node.Aggregations);

            if (node.Source != source ||
                node.Id != id ||
                node.Aggregations != aggregations)
            {
                node = new GroupExpression(node.CorrelationId, node.Type, source, id, aggregations);
            }

            return node;
        }

        /// <summary>
        /// Visits the grouped aggregate.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The GroupedAggregateExpression (possibly modified).</returns>
        protected virtual Expression VisitGroupedAggregate(GroupedAggregateExpression node)
        {
            var aggregate = (AggregationExpression)Visit(node.Aggregation);

            if (node.Aggregation != aggregate)
            {
                node =  new GroupedAggregateExpression(node.GroupCorrelationId, aggregate);
            }

            return node;
        }

        /// <summary>
        /// Visits the match.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The MatchExpression (possibly modified).</returns>
        protected virtual Expression VisitMatch(MatchExpression node)
        {
            var source = Visit(node.Source);
            var match = Visit(node.Match);

            if (node.Source != source || node.Match != match)
            {
                node = new MatchExpression(source, match);
            }

            return node;
        }

        /// <summary>
        /// Visits the pipeline.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The PipelineExpression (possibly modified).</returns>
        protected virtual Expression VisitPipeline(PipelineExpression node)
        {
            var source = Visit(node.Source);
            var projector = Visit(node.Projector);

            if (node.Source != source ||
                node.Projector != projector)
            {
                node = new PipelineExpression(source, projector, node.Aggregator);
            }

            return node;
        }

        /// <summary>
        /// Visits the project.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The ProjectExpression (possibly modified).</returns>
        protected virtual Expression VisitProject(ProjectExpression node)
        {
            var source = Visit(node.Source);
            var projector = Visit(node.Projector);

            if (node.Source != source ||
                node.Projector != projector)
            {
                node = new ProjectExpression(source, projector);
            }

            return node;
        }

        /// <summary>
        /// Visits the root aggregation.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The RootAggregationExpression (possibly modified).</returns>
        protected virtual Expression VisitRootAggregation(RootAggregationExpression node)
        {
            var projector = Visit(node.Projector);
            var source = Visit(node.Source);

            if (node.Source != source ||
                node.Projector != projector)
            {
                node = new RootAggregationExpression(source, projector, node.AggregationType);
            }

            return node;
        }

        /// <summary>
        /// Visits the skip limit.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The SkipLimitExpression (possibly modified).</returns>
        protected virtual Expression VisitSkipLimit(SkipLimitExpression node)
        {
            var source = Visit(node.Source);
            var skip = Visit(node.Skip);
            var limit = Visit(node.Limit);

            if (node.Source != source ||
                node.Skip != skip ||
                node.Limit != limit)
            {
                node = new SkipLimitExpression(source, skip, limit);
            }

            return node;
        }

        /// <summary>
        /// Visits the sort.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The SortExpression (possibly modified).</returns>
        protected virtual Expression VisitSort(SortExpression node)
        {
            var source = Visit(node.Source);
            var orderings = VisitSortClauses(node.SortClauses);

            if (node.Source != source || node.SortClauses != orderings)
            {
                node = new SortExpression(source, orderings);
            }

            return node;
        }

        /// <summary>
        /// Visits the sort clauses.
        /// </summary>
        /// <param name="sortClauses">The orderings.</param>
        /// <returns>The sort clauses (possibly modified).</returns>
        protected virtual ReadOnlyCollection<SortClause> VisitSortClauses(ReadOnlyCollection<SortClause> sortClauses)
        {
            if (sortClauses != null)
            {
                List<SortClause> alternate = null;
                for (int i = 0, n = sortClauses.Count; i < n; i++)
                {
                    var ordering = sortClauses[i];
                    var e = Visit(ordering.Expression);
                    if (alternate == null && e != ordering.Expression)
                    {
                        alternate = sortClauses.Take(i).ToList();
                    }
                    if (alternate != null)
                    {
                        alternate.Add(new SortClause(e, ordering.Direction));
                    }
                }
                if (alternate != null)
                {
                    sortClauses = alternate.AsReadOnly();
                }
            }
            return sortClauses;
        }
    }
}