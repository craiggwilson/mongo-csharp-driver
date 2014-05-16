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
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Translators
{
    internal class PipelineModelBuilder : LinqToMongoExpressionVisitor
    {
        // private static fields
        private static readonly string[] __projectingOperations = new[] { "$project", "$group" };

        // private fields
        private LambdaExpression _aggregator;
        private Type _documentType;
        private List<BsonDocument> _pipeline;
        private ExecutionModelProjection _projection;

        // public methods
        public ExecutionModel Build(PipelineExpression node)
        {
            _pipeline = new List<BsonDocument>();
            Visit(node);

            var model = new PipelineModel
            {
                Aggregator = _aggregator,
                DocumentType = _documentType,
                Pipeline = _pipeline,
                Projection = _projection
            };

            return model;
        }

        protected override Expression VisitCollection(CollectionExpression node)
        {
            _documentType = ((IQueryable)((ConstantExpression)node.Queryable).Value).ElementType;
            return node;
        }

        protected override Expression VisitDistinct(DistinctExpression node)
        {
            Visit(node.Source);

            var field = node.Projector as FieldExpression;
            if (field != null)
            {
                // in order to only get actual distinct values, we need to eliminate
                // any documents where the specified field doesn't exist.
                _pipeline.Add(new BsonDocument("$match",
                    new BsonDocument(field.SerializationInfo.ElementName,
                        new BsonDocument("$exists", true))));
            }

            var groupBody = new PipelineProjectTranslator().BuildGroupId(node.Projector);
            _pipeline.Add(new BsonDocument("$group", groupBody));

            return node;
        }

        protected override Expression VisitGroup(GroupExpression node)
        {
            Visit(node.Source);

            var id = new PipelineProjectTranslator().BuildGroupId(node.Id);
            var doc = new PipelineProjectTranslator().BuildAggregations(node.Aggregations);

            _pipeline.Add(new BsonDocument("$group", id.Merge(doc)));

            return node;
        }

        protected override Expression VisitMatch(MatchExpression node)
        {
            Visit(node.Source);

            var predicateTranslator = new PredicateTranslator();
            var query = predicateTranslator.BuildQuery(node.Match);

            _pipeline.Add(new BsonDocument("$match", query.ToBsonDocument()));

            return node;
        }

        protected override Expression VisitPipeline(PipelineExpression node)
        {
            _aggregator = node.Aggregator;

            Visit(node.Source);

            // If there are any pipeline operators that change the shape of the result
            // we use the pipeline projection builder. Otherwise, we'll project as if it
            // was a query.
            if (_pipeline.Any(x => __projectingOperations.Contains(x.GetElement(0).Name)))
            {
                _projection = new PipelineProjectionBuilder().Build(node.Projector, _documentType);
            }
            else
            {
                _projection = new QueryProjectionBuilder().Build(node.Projector, _documentType);
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

                    _pipeline.Add(new BsonDocument("$project", builder.ToBsonDocument()));
                }
            }

            return node;
        }

        protected override Expression VisitProject(ProjectExpression node)
        {
            Visit(node.Source);

            var doc = new PipelineProjectTranslator().BuildProject(node.Projector);

            _pipeline.Add(new BsonDocument("$project", doc));

            return node;
        }

        protected override Expression VisitRootAggregation(RootAggregationExpression node)
        {
            Visit(node.Source);

            var id = new BsonDocument("_id", 1);
            switch (node.AggregationType)
            {
                case RootAggregationType.Count:
                    var count = new PipelineProjectTranslator().BuildAggregations(new[] { node.Projector });
                    _pipeline.Add(new BsonDocument("$group", id.Merge(count)));
                    break;
                case RootAggregationType.Average:
                case RootAggregationType.Max:
                case RootAggregationType.Min:
                case RootAggregationType.Sum:
                    var field = AddProjectionIfNecessary((FieldExpression)node.Projector);
                    var min = new PipelineProjectTranslator().BuildAggregations(new[] { field });
                    _pipeline.Add(new BsonDocument("$group", id.Merge(min)));
                    break;
                default:
                    throw LinqErrors.Unsupported();
            }
            return node;
        }

        protected override Expression VisitSkipLimit(SkipLimitExpression node)
        {
            Visit(node.Source);
            if (node.Skip != null)
            {
                _pipeline.Add(new BsonDocument("$skip", (int)((ConstantExpression)node.Skip).Value));
            }
            if (node.Limit != null)
            {
                _pipeline.Add(new BsonDocument("$limit", (int)((ConstantExpression)node.Limit).Value));
            }

            return node;
        }

        protected override Expression VisitSort(SortExpression node)
        {
            Visit(node.Source);

            var sortBy = new SortTranslator().BuildSortBy(node.SortClauses);
            _pipeline.Add(new BsonDocument("$sort", sortBy.ToBsonDocument()));

            return node;
        }

        // private methods
        private Expression AddProjectionIfNecessary(FieldExpression node)
        {
            // TODO: maybe change the node.Projector type to FieldExpression.
            var aggregation = (AggregationExpression)node.Expression;
            var argument = aggregation.Argument as FieldExpression;
            if (argument == null)
            {
                // If the node's projector is not a field expression, we need to artificially create
                // one so that we can apply the aggregation to it...
                var generated = new PipelineProjectTranslator(false).BuildValue(aggregation.Argument);

                _pipeline.Add(new BsonDocument("$project", new BsonDocument("_aggProj0", generated)));
                argument = new FieldExpression(
                    aggregation.Argument,
                    node.SerializationInfo.WithNewName("_aggProj0"),
                    true);
                aggregation = new AggregationExpression(aggregation.Type, aggregation.AggregationType, argument);
                return new FieldExpression(
                    aggregation,
                    node.SerializationInfo,
                    true);
            }

            return node;
        }
    }
}