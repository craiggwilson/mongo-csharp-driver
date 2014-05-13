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

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections.ObjectModel;

namespace MongoDB.Driver.Linq.Translators
{
    /// <summary>
    /// Builds the projection for an AggregationQueryModel.
    /// </summary>
    internal class PipelineProjectionBuilder : LinqToMongoExpressionVisitor
    {
        // private fields
        private List<BsonSerializationInfo> _serializationInfo;
        private ParameterExpression _parameter;

        // public methods
        /// <summary>
        /// Builds the projection.
        /// </summary>
        /// <param name="projector">The projector.</param>
        /// <param name="documentType">Type of the document.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Visits the document.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The DocumentExpression (possibly modified).</returns>
        protected override Expression VisitDocument(DocumentExpression node)
        {
            return Visit(node.Expression);
        }

        /// <summary>
        /// Visits the field.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The FieldExpression (possibly modified).</returns>
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

        /// <summary>
        /// Visits a ParameterExpression.
        /// </summary>
        /// <param name="node">The ParameterExpression.</param>
        /// <returns>The ParameterExpression (possibly modified).</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            // all parameters left in the tree will always be the document.
            return _parameter;
        }
    }
}