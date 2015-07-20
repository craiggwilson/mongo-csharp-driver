/* Copyright 2010-2015 MongoDB Inc.
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
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Relinq.Preparation.Pipeline.ResultOperatorHandlers;
using MongoDB.Driver.Relinq.Structure;
using MongoDB.Driver.Relinq.Structure.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace MongoDB.Driver.Relinq.Preparation.Pipeline
{
    internal class PipelineBuildingQueryModelVisitor : QueryModelVisitorBase
    {
        private static readonly PipelineResultOperatorHandlerRegistry __resultOperatorHandlerRegistry;

        static PipelineBuildingQueryModelVisitor()
        {
            __resultOperatorHandlerRegistry = new PipelineResultOperatorHandlerRegistry();
            __resultOperatorHandlerRegistry.Register(new DistinctResultOperatorHandler());
            __resultOperatorHandlerRegistry.Register(new GroupResultOperatorHandler());
        }

        public static PipelineModel Prepare(
            QueryModel queryModel,
            IBsonSerializer rootSerializer,
            IBsonSerializerRegistry serializerRegistry)
        {
            var binder = new PipelineBuildingQueryModelVisitor(rootSerializer, serializerRegistry);
            binder.VisitQueryModel(queryModel);
            return binder._builder.Build();
        }

        private readonly IBsonSerializer _rootSerializer;
        private readonly PipelineBuilder _builder;
        private readonly PipelinePreparationContext _context;

        private PipelineBuildingQueryModelVisitor(IBsonSerializer rootSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            _rootSerializer = rootSerializer;
            _context = new PipelinePreparationContext(rootSerializer, serializerRegistry, new UniqueIdentifierGenerator());
            _builder = new PipelineBuilder();
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            var subQueryExpression = fromClause.FromExpression as SubQueryExpression;
            if (subQueryExpression != null)
            {
                subQueryExpression.QueryModel.Accept(this);
            }
            else
            {
                var constantExpression = fromClause.FromExpression as ConstantExpression;
                if (constantExpression != null &&
                    constantExpression.Type.IsGenericType &&
                    constantExpression.Type.GetGenericTypeDefinition() == typeof(RelinqQueryable<>))
                {
                    _builder.CurrentProjector = new DocumentExpression(_rootSerializer);
                }
            }

            _context.AddExpressionMapping(
                new QuerySourceReferenceExpression(fromClause),
                _builder.CurrentProjector);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            var resultOperatorType = resultOperator.GetType();

            IPipelineResultOperatorHandler handler;
            if (!__resultOperatorHandlerRegistry.TryGet(resultOperatorType, out handler))
            {
                var message = string.Format("The result operator '{0}' is not supported.", resultOperatorType);
                throw new NotSupportedException(message);
            }

            handler.Handle(resultOperator, _builder, _context);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            var selector = _context.PrepareSelectExpression(selectClause.Selector);
            if (selector == _builder.CurrentProjector)
            {
                return;
            }

            _builder.AddProjectStage(selector);

            var projector = selector;
            var documentWrappedFieldExpression = projector as DocumentWrappedFieldExpression;
            if (documentWrappedFieldExpression != null)
            {
                projector = new FieldExpression(
                    documentWrappedFieldExpression.FieldName,
                    documentWrappedFieldExpression.Serializer);
            }

            _builder.CurrentProjector = projector;
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            var predicate = _context.PrepareWhereExpression(whereClause.Predicate);
            _builder.AddMatchStage(predicate);
        }
    }
}
