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
using MongoDB.Driver.Relinq.Structure.Expressions;
using MongoDB.Driver.Relinq.Structure.Stages;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace MongoDB.Driver.Relinq.Preparation.Pipeline.ResultOperatorHandlers
{
    internal class DistinctResultOperatorHandler : IPipelineResultOperatorHandler
    {
        public Type SupportedResultOperatorType
        {
            get { return typeof(DistinctResultOperator); }
        }

        public void Handle(ResultOperatorBase resultOperator, PipelineBuilder builder, PipelinePreparationContext context)
        {
            Expression idSelector;
            ProjectStage previousProjectStage;
            if (builder.TryRemoveLastStage<ProjectStage>(out previousProjectStage))
            {
                idSelector = previousProjectStage.Selector;
            }
            else
            {
                var currentProjector = (SerializationExpression)builder.CurrentProjector;
                idSelector = new FieldExpression("$ROOT", currentProjector.Serializer);
            }

            var documentWrappedSelector = idSelector as DocumentWrappedFieldExpression;
            if (documentWrappedSelector != null)
            {
                idSelector = documentWrappedSelector.Expression;
            }

            var stage = new GroupStage(idSelector);
            builder.AddGroupStage(idSelector);

            var projector = context.WrapField(idSelector, "_id");
            builder.CurrentProjector = new FieldExpression("_id", projector.Serializer);
        }
    }
}
