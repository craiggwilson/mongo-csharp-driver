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

using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.PipelineOperationBinders
{
    internal class ProjectBinder : ProjectingPipelineOperationBinder
    {
        public static readonly string ScalarProjectionFieldName = "_fld0";

        public ProjectBinder(Dictionary<Expression, GroupExpression> groupMap)
            : base(groupMap)
        { }

        // public methods
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
                        projector.Type);
                    return new FieldExpression(projector, info, true);
            }
        }
    }
}