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

namespace MongoDB.Driver.Linq.Translators
{
    internal class PipelineProjectionBuilder : LinqToMongoExpressionVisitor
    {
        // private fields
        private List<BsonSerializationInfo> _serializationInfo;
        private ParameterExpression _parameter;

        // public methods
        public ExecutionModelProjection Build(Expression projector, Type documentType)
        {
            if (projector.Type.IsGenericType &&
                projector.Type.GetGenericTypeDefinition() == typeof(Grouping<,>) &&
                projector.Type.GetGenericArguments()[1] == documentType)
            {
                throw LinqErrors.InvalidProjection(projector);
            }

            var fields = new FieldGatherer().Gather(projector, true);
            if (fields.Count > 0)
            {
                _parameter = Expression.Parameter(typeof(IProjectionValueStore), "document");
            }
            else
            {
                // we are projecting the entire document, so no store needs to be involved.
                _parameter = Expression.Parameter(documentType, "document");
            }

            _serializationInfo = new List<BsonSerializationInfo>();
            return new ExecutionModelProjection
            {
                FieldSerializationInfo = _serializationInfo,
                Projector = Expression.Lambda(Visit(projector), _parameter)
            };
        }

        // protected methods
        protected override Expression VisitDocument(DocumentExpression node)
        {
            return Visit(node.Expression);
        }

        protected override Expression VisitField(FieldExpression node)
        {
            _serializationInfo.Add(node.SerializationInfo);
            return Expression.Call(
                _parameter,
                "GetValue",
                new[] { node.SerializationInfo.NominalType },
                Expression.Constant(node.SerializationInfo.ElementName),
                Expression.Constant(null, typeof(object)));
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // all parameters left in the tree will always be the document.
            return _parameter;
        }
    }
}