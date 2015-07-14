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
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Relinq.Structure;
using MongoDB.Driver.Relinq.Structure.Expressions;
using MongoDB.Driver.Relinq.Structure.Stages;
using Remotion.Linq;

namespace MongoDB.Driver.Relinq.Language
{
    internal class PipelineLanguageTranslator : QueryModelVisitorBase
    {
        public static RenderedPipelineDefinition<T> Translate<T>(PipelineModel pipeline)
        {
            var visitor = new PipelineLanguageTranslator();
            visitor.Translate(pipeline);

            var serializationExpression = pipeline.Projector as SerializationExpression;

            return new RenderedPipelineDefinition<T>(visitor._stages, (IBsonSerializer<T>)serializationExpression.Serializer);
        }

        private List<BsonDocument> _stages;

        private PipelineLanguageTranslator()
        {
            _stages = new List<BsonDocument>();
        }

        private void Translate(PipelineModel model)
        {
            foreach (var stage in model.Stages)
            {
                switch (stage.StageType)
                {
                    case PipelineStageType.Group:
                        TranslateGroup((GroupStage)stage);
                        break;
                    case PipelineStageType.Match:
                        TranslateMatch((MatchStage)stage);
                        break;
                    case PipelineStageType.Project:
                        TranslateProject((ProjectStage)stage);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private void TranslateGroup(GroupStage groupStage)
        {
            var value = AggregateLanguageBuildingExpressionVisitor.Build(groupStage.IdSelector);
            _stages.Add(new BsonDocument("$group", new BsonDocument("_id", value)));
        }

        private void TranslateMatch(MatchStage stage)
        {
            var value = MatchBuildingExpressionVisitor.Build(stage.Predicate);
            _stages.Add(new BsonDocument("$match", value));
        }

        private void TranslateProject(ProjectStage stage)
        {
            BsonDocument value;
            if (stage.Selector is FieldExpression)
            {
                value = new BsonDocument(((IFieldExpression)stage.Selector).FieldName, 1);
            }
            else
            {
                var result = AggregateLanguageBuildingExpressionVisitor.Build(stage.Selector);
                if (result.BsonType == BsonType.String)
                {
                    value = new BsonDocument(result.ToString(), 1);
                }
                else if (result.BsonType == BsonType.Document)
                {
                    value = (BsonDocument)result;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            if (!value.Contains("_id"))
            {
                value.Add("_id", 0);
            }
            _stages.Add(new BsonDocument("$project", value));
        }

    }
}