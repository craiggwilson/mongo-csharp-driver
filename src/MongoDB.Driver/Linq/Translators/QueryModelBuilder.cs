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

using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Driver.Builders;

namespace MongoDB.Driver.Linq.Translators
{
    /// <summary>
    /// Attempts to build a <see cref="QueryModel"/> from a PipelineExpression.
    /// </summary>
    internal class QueryModelBuilder : LinqToMongoExpressionVisitor
    {
        // private fields
        private LambdaExpression _aggregator;
        private Type _countType;
        private BsonSerializationInfo _distinctValueSerializationInfo;
        private Type _documentType;
        private bool _isDistinct;
        private IMongoFields _fields;
        private bool _isCount;
        private int? _numberToLimit;
        private int? _numberToSkip;
        private IMongoQuery _query;
        private ExecutionModelProjection _projection;
        private IMongoSortBy _sortBy;
        private Expression _root;

        // public methods
        /// <summary>
        /// Attempts to build a <see cref="QueryModel"/>.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>A <see cref="QueryModel"/> or null if the result can't be expression using a mongodb query.</returns>
        public ExecutionModel Build(PipelineExpression node)
        {
            Visit(node);

            var model = new QueryModel()
            {
                Aggregator = _aggregator,
                CountType = _countType,
                DistinctValueSerializationInfo = _distinctValueSerializationInfo,
                DocumentType = _documentType,
                Fields = _fields,
                IsCount = _isCount,
                IsDistinct = _isDistinct,
                NumberToLimit = _numberToLimit,
                NumberToSkip = _numberToSkip,
                Query = _query,
                Projection = _projection,
                SortBy = _sortBy
            };

            return model;
        }

        // protected methods
        /// <summary>
        /// Visits the collection.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The CollectionExpression (possibly modified).</returns>
        protected override Expression VisitCollection(CollectionExpression node)
        {
            _documentType = ((IQueryable)((ConstantExpression)node.Queryable).Value).ElementType;
            return node;
        }

        protected override Expression VisitDistinct(DistinctExpression node)
        {
            if(_root != node)
            {
                // Distinct must be the last operator when using the 
                // query model.
                throw LinqErrors.Unsupported();
            }

            Visit(node.Source);

            var field = node.Projector as FieldExpression;
            if (field != null)
            {
                // For the query model, distinct *MUST* be a field
                _distinctValueSerializationInfo = field.SerializationInfo;
                _isDistinct = true;

                var serializationInfo = field.SerializationInfo;
                var parameter = Expression.Parameter(serializationInfo.NominalType, "value");

                _projection = new ExecutionModelProjection
                {
                    Projector = Expression.Lambda(parameter, parameter)
                };
                return node;
            }

            // TODO: Better Distinct Failure method...
            throw LinqErrors.Unsupported();
        }

        /// <summary>
        /// Visits the group.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The GroupExpression (possibly modified).</returns>
        protected override Expression VisitGroup(GroupExpression node)
        {
            throw LinqErrors.Unsupported();
        }

        /// <summary>
        /// Visits the match.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The MatchExpression (possibly modified).</returns>
        protected override Expression VisitMatch(MatchExpression node)
        {
            Visit(node.Source);

            // simple queries only support a single "query"
            if (_query != null)
            {
                throw LinqErrors.Unsupported();
            }

            // simple queries cannot use computed fields
            if (new ProjectedFieldIndicator().HasProjectedFields(node.Match))
            {
                throw LinqErrors.Unsupported();
            }

            var predicateTranslator = new PredicateTranslator();
            _query = predicateTranslator.BuildQuery(node.Match);

            return node;
        }

        /// <summary>
        /// Visits the pipeline.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The PipelineExpression (possibly modified).</returns>
        protected override Expression VisitPipeline(PipelineExpression node)
        {
            // TODO: there should never be two result expressions in the tree.
            // it might be better to throw an exception as something
            // has gone horribly wrong...
            if (_root != null)
            {
                throw LinqErrors.Unsupported();
            }

            _root = node.Source;
            _aggregator = node.Aggregator;

            Visit(node.Source);

            if (_projection == null)
            {
                _projection = new QueryProjectionBuilder().Build(node.Projector, _documentType);
            }
            else if (_projection.Projector.Body.Type != node.Projector.Type)
            {
                // this exists when we created a projection in VisitGroup, but that projection
                // turns out to be computed the wrong type.  This query can be rendered using
                // aggregation framework though...
                throw LinqErrors.Unsupported();
            }

            if (_projection.HasFields)
            {
                var builder = new FieldsBuilder();
                bool hasId = false;
                foreach (var field in _projection.FieldSerializationInfo)
                {
                    if (field.ElementName == "_id")
                    {
                        hasId = true;
                    }
                    builder.Include(field.ElementName);
                }

                // don't bring back the _id unless it was asked for...
                if (!hasId)
                {
                    builder.Exclude("_id");
                }

                _fields = builder;
            }


            return node;
        }

        /// <summary>
        /// Visits the project.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The ProjectExpression (possibly modified).</returns>
        protected override Expression VisitProject(ProjectExpression node)
        {
            Visit(node.Source);

            // Simple queries only support simple projections.  Simples projections
            // can be determined by testing whether the projector only contains
            // simple field references.
            var nominator = new ExpressionNominator(e =>
                !(e is DocumentExpression) ||
                !(e is FieldExpression) ||
                !((FieldExpression)e).IsProjected);

            var candidates = nominator.Nominate(node.Projector);

            if (!candidates.Contains(node.Projector))
            {
                throw LinqErrors.Unsupported();
            }

            return node;
        }

        /// <summary>
        /// Visits the root aggregation.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The RootAggregationExpression (possibly modified).</returns>
        protected override Expression VisitRootAggregation(RootAggregationExpression node)
        {
            Visit(node.Source);

            switch (node.AggregationType)
            {
                case RootAggregationType.Count:
                    _isCount = true;
                    _countType = node.Type;
                    return node;
                case RootAggregationType.Max:
                case RootAggregationType.Min:
                    var field = node.Projector as FieldExpression;
                    if (field != null)
                    {
                        var aggregation = field.Expression as AggregationExpression;
                        if (aggregation != null)
                        {
                            var sortClause = new SortClause(
                                aggregation.Argument,
                                aggregation.AggregationType == AggregationType.Max ? SortDirection.Descending : SortDirection.Ascending);

                            VisitSortClauses(new List<SortClause> { sortClause }.AsReadOnly());
                            _numberToLimit = 1;
                            var subField = (FieldExpression)aggregation.Argument;
                            var store = Expression.Parameter(typeof(IProjectionValueStore), "store");
                            var projector = Expression.Lambda(
                                Expression.Call(
                                    store,
                                    "GetValue",
                                    new[] { subField.SerializationInfo.NominalType },
                                    Expression.Constant(subField.SerializationInfo.ElementName),
                                    Expression.Constant(null, typeof(object))),
                                store);

                            _projection = new ExecutionModelProjection
                            {
                                FieldSerializationInfo = new[] { subField.SerializationInfo },
                                Projector = projector
                            };
                            return node;
                        }
                    }
                    break;
            }

            throw LinqErrors.Unsupported();
        }

        /// <summary>
        /// Visits the skip limit.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The SkipLimitExpression (possibly modified).</returns>
        protected override Expression VisitSkipLimit(SkipLimitExpression node)
        {
            Visit(node.Source);

            // queries only support a single skip/limit and cannot follow a distinct
            if (_isDistinct || _numberToSkip.HasValue || _numberToLimit.HasValue)
            {
                throw LinqErrors.Unsupported();
            }

            if (node.Limit != null)
            {
                _numberToLimit = (int)((ConstantExpression)node.Limit).Value;
            }
            if (node.Skip != null)
            {
                _numberToSkip = (int)((ConstantExpression)node.Skip).Value;
            }

            return node;
        }

        /// <summary>
        /// Visits the sort.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The SortExpression (possibly modified).</returns>
        protected override Expression VisitSort(SortExpression node)
        {
            Visit(node.Source);

            // simple queries only support a single sort and cannot follow a distinct...
            if (_isDistinct || _sortBy != null)
            {
                throw LinqErrors.Unsupported();
            }

            VisitSortClauses(node.SortClauses);
            return node;
        }

        /// <summary>
        /// Visits the sort clauses.
        /// </summary>
        /// <param name="sortClauses">The orderings.</param>
        /// <returns>The sort clauses (possibly modified).</returns>
        protected override ReadOnlyCollection<SortClause> VisitSortClauses(ReadOnlyCollection<SortClause> sortClauses)
        {
            var hasProjectedFields = new ProjectedFieldIndicator().HasProjectedFields(sortClauses);
            if (hasProjectedFields)
            {
                throw LinqErrors.Unsupported();
            }

            _sortBy = new SortTranslator().BuildSortBy(sortClauses);
            return sortClauses;
        }

        // private methods
        private bool IsMinOrMaxAggregate(AggregationExpression aggregation)
        {
            return aggregation != null &&
                (aggregation.AggregationType == AggregationType.Max || aggregation.AggregationType == AggregationType.Min) &&
                aggregation.Argument != null &&
                aggregation.Argument is FieldExpression &&
                _sortBy == null &&
                _numberToLimit == null &&
                _numberToSkip == null;
        }

        private class ProjectedFieldIndicator : LinqToMongoExpressionVisitor
        {
            private bool _hasProjectedFields;

            public bool HasProjectedFields(Expression node)
            {
                _hasProjectedFields = false;
                Visit(node);
                return _hasProjectedFields;
            }

            public bool HasProjectedFields(ReadOnlyCollection<SortClause> sortClauses)
            {
                _hasProjectedFields = false;
                VisitSortClauses(sortClauses);
                return _hasProjectedFields;
            }

            protected override Expression Visit(Expression node)
            {
                if (_hasProjectedFields)
                {
                    return node;
                }

                return base.Visit(node);
            }

            protected override Expression VisitField(FieldExpression node)
            {
                _hasProjectedFields = _hasProjectedFields || node.IsProjected;
                return node;
            }

            protected override ReadOnlyCollection<SortClause> VisitSortClauses(ReadOnlyCollection<SortClause> sortClauses)
            {
                foreach (var clause in sortClauses)
                {
                    Visit(clause.Expression);
                    if (_hasProjectedFields)
                    {
                        break;
                    }
                }

                return sortClauses;
            }
        }
    }
}