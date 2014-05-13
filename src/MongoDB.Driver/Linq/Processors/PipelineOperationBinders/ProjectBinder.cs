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

using MongoDB.Driver.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Linq.Processors.PipelineOperationBinders
{
    /// <summary>
    /// Binds project operations.
    /// </summary>
    internal class ProjectBinder : ProjectingPipelineOperationBinder
    {
        public static readonly string ScalarProjectionFieldName = "_fld0";

        public ProjectBinder(Dictionary<Expression, GroupExpression> groupMap)
            : base(groupMap)
        { }

        // public methods
        /// <summary>
        /// Binds a Project operation.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="selector">The selector.</param>
        /// <returns></returns>
        public Expression Bind(PipelineExpression pipeline, LambdaExpression selector)
        {
            RegisterProjector(pipeline.Projector);
            RegisterParameterReplacement(selector.Parameters[0], pipeline.Projector);

            var projector = Visit(selector.Body);

            var pipelineProjector = BuildPipelineProjector(projector);

            return new PipelineExpression(
                new ProjectExpression(pipeline.Source, projector),
                pipelineProjector);
        }

        // private methods
        private Expression BuildPipelineProjector(Expression projector)
        {
            switch (projector.NodeType)
            {
                case (ExpressionType)LinqToMongoExpressionType.Field:
                case (ExpressionType)LinqToMongoExpressionType.Document:
                    return projector;
                case ExpressionType.New:
                    return FlattenNewExpression((NewExpression)projector);
                default:
                    var serializer = BsonSerializer.LookupSerializer(projector.Type);
                    var info = new BsonSerializationInfo(
                        ScalarProjectionFieldName, // MAGIC STRING!!! It isn't referenced anywhere else in the code.
                        serializer,
                        projector.Type,
                        serializer.GetDefaultSerializationOptions());
                    return new FieldExpression(projector, info, true);
            }
        }
    }
}