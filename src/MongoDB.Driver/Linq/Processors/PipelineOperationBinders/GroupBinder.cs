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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MongoDB.Driver.Linq.Processors.PipelineOperationBinders
{
    /// <summary>
    /// Binds group operations.
    /// </summary>
    internal class GroupBinder : ProjectingPipelineOperationBinder
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupBinder" /> class.
        /// </summary>
        /// <param name="groupMap">The group map.</param>
        public GroupBinder(Dictionary<Expression, GroupExpression> groupMap)
            : base(groupMap)
        { }

        // public methods
        /// <summary>
        /// Binds the distinct.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <returns>An expression representing the grouping.</returns>
        public Expression BindDistinct(PipelineExpression pipeline)
        {
            var distinct = new DistinctExpression(pipeline.Source, pipeline.Projector);

            return new PipelineExpression(
                distinct,
                BuildIdField(pipeline.Projector),
                pipeline.Aggregator);
        }

        /// <summary>
        /// Binds a GroupBy operation.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>An expression representing the grouping.</returns>
        public Expression BindGroupBy(PipelineExpression pipeline, LambdaExpression keySelector)
        {
            RegisterProjector(pipeline.Projector);
            RegisterParameterReplacement(keySelector.Parameters[0], pipeline.Projector);
            var keyExpression = Visit(keySelector.Body);

            var pipelineProjectorType = GetPipelineProjectorType(pipeline.Projector.Type);
            var groupingType = typeof(Grouping<,>).MakeGenericType(keyExpression.Type, pipelineProjectorType);

            var groupExpression = new GroupExpression(
                Guid.NewGuid(),
                typeof(IEnumerable<>).MakeGenericType(typeof(IGrouping<,>).MakeGenericType(keyExpression.Type, pipelineProjectorType)),
                pipeline.Source,
                keyExpression,
                Enumerable.Empty<Expression>());

            var projector = Expression.New(
                groupingType.GetConstructors()[0],
                new[] 
                { 
                    BuildIdField(keyExpression), 
                    pipeline.Projector
                });

            RegisterGroup(projector, groupExpression);

            return new PipelineExpression(
                groupExpression,
                projector);
        }

        // private methods
        private Type GetPipelineProjectorType(Type pipelineProjectorType)
        {
            if (pipelineProjectorType.IsGenericType && pipelineProjectorType.GetGenericTypeDefinition() == typeof(Grouping<,>))
            {
                pipelineProjectorType = typeof(IGrouping<,>).MakeGenericType(pipelineProjectorType.GetGenericArguments());
            }

            return pipelineProjectorType;
        }

        private FieldExpression BuildIdField(Expression key)
        {
            var doc = key as DocumentExpression;
            if (doc != null && doc.IsProjected)
            {
                return new FieldExpression(
                    doc.Expression,
                    doc.SerializationInfo.WithNewName("_id"),
                    true);
            }

            var id = key as FieldExpression;
            if (id != null)
            {
                return new FieldExpression(
                    id.Expression,
                    id.SerializationInfo.WithNewName("_id"),
                    true);
            }
            else if(key.NodeType == ExpressionType.New)
            {
                key = FlattenNewExpression((NewExpression)key);
                var serializer = LookupSerializer((NewExpression)key);
                return new FieldExpression(
                    key,
                    new BsonSerializationInfo(
                        "_id",
                        serializer,
                        key.Type,
                        serializer.GetDefaultSerializationOptions()),
                    true);
            }
            else if (key.NodeType == ExpressionType.Constant || key.NodeType == ExpressionType.Parameter)
            {
                var serializer = BsonSerializer.LookupSerializer(key.Type);
                return new FieldExpression(
                    key,
                    new BsonSerializationInfo(
                        "_id",
                        serializer,
                        key.Type,
                        serializer.GetDefaultSerializationOptions()),
                    true);
            }

            throw LinqErrors.UnsupportedGroupingKey(key);
        }

    }
}