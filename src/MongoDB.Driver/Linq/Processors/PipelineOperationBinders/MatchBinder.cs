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
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.PipelineOperationBinders
{
    internal class MatchBinder : GroupAwarePipelineOperationBinder
    {
        // constructors
        public MatchBinder(Dictionary<Expression, GroupExpression> groupMap)
            : base(groupMap)
        { }

        // public methods
        public Expression Bind(PipelineExpression pipeline, LambdaExpression predicate)
        {
            RegisterProjector(pipeline.Projector);
            RegisterParameterReplacement(predicate.Parameters[0], pipeline.Projector);

            var predicateExpression = Visit(predicate.Body);

            return new PipelineExpression(
                new MatchExpression(
                    pipeline.Source,
                    predicateExpression),
                pipeline.Projector);
        }

        public Expression BindOfType(PipelineExpression result, Type type)
        {
            // OfType is two operations in one. First we create MatchExpression
            // for the test.  Second, we want to issue a conversion projection 
            // using the selector parameter of a PipelineExpression such that
            // the conversion will be propogated to all future expressions.

            var documentType = result.Projector.Type;

            var parameter = Expression.Parameter(documentType, "doc");
            var predicate = Expression.Lambda(
                Expression.TypeIs(
                    parameter,
                    type),
                parameter);

            result = (PipelineExpression)Bind(result, predicate);

            var selectorParameter = Expression.Parameter(documentType, "doc");
            var selector = Expression.Lambda(
                Expression.Convert(
                    selectorParameter,
                    type),
                selectorParameter);

            var serializer = BsonSerializer.LookupSerializer(type);
            var info = new BsonSerializationInfo(null, serializer, type);
            var projector = new DocumentExpression(
                selector.Body,
                info,
                false);

            return new PipelineExpression(
                result.Source,
                projector,
                result.Aggregator);
        }
    }
}