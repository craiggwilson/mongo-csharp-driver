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
* 
*/

using System;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors;

namespace MongoDB.Driver.Linq.Translators
{
    internal static class AggregateProjectTranslator
    {
        public static RenderedProjectionDefinition<TResult> Translate<TDocument, TResult>(Expression<Func<TDocument, TResult>> projector, IBsonSerializer<TDocument> parameterSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var bindingContext = new PipelineBindingContext(serializerRegistry);
            var parameterExpression = new DocumentExpression(parameterSerializer);
            bindingContext.AddExpressionMapping(projector.Parameters[0], parameterExpression);
            var normalizedBody = Transformer.Transform(projector.Body);
            var evaluatedBody = PartialEvaluator.Evaluate(normalizedBody);
            var bound = bindingContext.Bind(evaluatedBody);

            var projectionSerializer = bindingContext.GetSerializer(bound.Type, bound);
            var projection = TranslateProject(bound);

            return new RenderedProjectionDefinition<TResult>(projection, (IBsonSerializer<TResult>)projectionSerializer);
        }

        public static BsonDocument TranslateProject(Expression expression)
        {
            var value = AggregateLanguageTranslator.Translate(expression);
            var projection = value as BsonDocument;
            if (projection == null)
            {
                projection = new BsonDocument(value.ToString().Substring(1), 1);
            }
            else if (expression.NodeType != ExpressionType.New && expression.NodeType != ExpressionType.MemberInit)
            {
                // this means we are scalar projection
                projection = new BsonDocument("__fld0", value);
            }

            if (!projection.Contains("_id"))
            {
                projection.Add("_id", 0);
            }

            return projection;
        }

    }
}
