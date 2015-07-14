/* Copyright 2015 MongoDB Inc.
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
using MongoDB.Driver.Relinq.Preparation.Embedded.ResultOperatorHandlers;
using MongoDB.Driver.Relinq.Structure.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace MongoDB.Driver.Relinq.Preparation.Embedded
{
    internal class EmbeddedPipelineBuildingQueryModelVisitor : QueryModelVisitorBase
    {
        private static readonly EmbeddedPipelineResultOperatorHandlerRegistry __resultOperatorHandlerRegistry;

        static EmbeddedPipelineBuildingQueryModelVisitor()
        {
            __resultOperatorHandlerRegistry = new EmbeddedPipelineResultOperatorHandlerRegistry();
            __resultOperatorHandlerRegistry.Register(new AnyResultOperatorHandler());
        }

        public static Expression Prepare(
            QueryModel queryModel,
            IPipelinePreparationContext parentContext)
        {
            var visitor = new EmbeddedPipelineBuildingQueryModelVisitor(parentContext);
            visitor.VisitQueryModel(queryModel);
            return visitor._builder.CurrentExpression;
        }

        private readonly EmbeddedPipelinePreparationContext _context;
        private EmbeddedPipelineBuilder _builder;

        private EmbeddedPipelineBuildingQueryModelVisitor(IPipelinePreparationContext parentContext)
        {
            _context = new EmbeddedPipelinePreparationContext(parentContext);
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
                var fromExpression = _context.PrepareFromExpression(fromClause.FromExpression);
                _builder = new EmbeddedPipelineBuilder(fromExpression, fromClause.ItemName);
                var fieldExpression = fromExpression as IFieldExpression;
                if (fieldExpression == null)
                {
                    // TODO: I think this is legal when it's a constant collection of items, like a local array
                    throw new NotSupportedException();
                }
                else
                {
                    BsonSerializationInfo itemSerializationInfo;
                    var arraySerializer = fieldExpression.Serializer as IBsonArraySerializer;
                    if (arraySerializer == null || !arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                    {
                        throw new NotSupportedException();
                    }

                    _builder.CurrentProjector = new ArrayItemExpression(fieldExpression, itemSerializationInfo.Serializer);
                }
            }

            _context.AddExpressionMapping(
                new QuerySourceReferenceExpression(fromClause),
                _builder.CurrentProjector);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            var resultOperatorType = resultOperator.GetType();

            IEmbeddedPipelineResultOperatorHandler handler;
            if (!__resultOperatorHandlerRegistry.TryGet(resultOperatorType, out handler))
            {
                var message = string.Format("The result operator '{0}' is not supported.", resultOperatorType);
                throw new NotSupportedException(message);
            }

            handler.Handle(resultOperator, _builder, _context);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            return;
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            var predicate = _context.PrepareWhereExpression(whereClause.Predicate);
            _builder.CurrentExpression = new FilterExpression(
                _builder.CurrentExpression,
                queryModel.MainFromClause.ItemName,
                predicate);
        }
    }
}
