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
    internal class LinqToMongoExpressionVisitor : ExpressionVisitor
    {
        protected LambdaExpression GetLambda(Expression node)
        {
            return (LambdaExpression)StripQuotes(node);
        }

        protected bool IsLambda(Expression node)
        {
            return StripQuotes(node).NodeType == ExpressionType.Lambda;
        }

        protected bool IsLambda(Expression node, int parameterCount)
        {
            var lambda = StripQuotes(node);
            return lambda.NodeType == ExpressionType.Lambda &&
                ((LambdaExpression)lambda).Parameters.Count == parameterCount;
        }

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

        protected Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

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

        protected virtual Expression VisitAggregation(AggregationExpression node)
        {
            var argument = Visit(node.Argument);
            if (node.Argument != argument)
            {
                node = new AggregationExpression(node.Type, node.AggregationType, argument);
            }

            return node;
        }

        protected virtual Expression VisitCollection(CollectionExpression node)
        {
            var queryable = Visit(node.Queryable);
            if (node.Queryable != queryable)
            {
                node = new CollectionExpression(node.DocumentType, queryable);
            }

            return node;
        }

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

        protected virtual Expression VisitDocument(DocumentExpression node)
        {
            return node;
        }

        protected virtual Expression VisitField(FieldExpression node)
        {
            return node;
        }

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

        protected virtual Expression VisitGroupedAggregate(GroupedAggregateExpression node)
        {
            var aggregate = (AggregationExpression)Visit(node.Aggregation);

            if (node.Aggregation != aggregate)
            {
                node =  new GroupedAggregateExpression(node.GroupCorrelationId, aggregate);
            }

            return node;
        }

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