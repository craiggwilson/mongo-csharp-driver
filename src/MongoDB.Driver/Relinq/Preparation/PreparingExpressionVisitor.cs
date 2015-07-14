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

using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Relinq.Preparation.Embedded;
using MongoDB.Driver.Relinq.Structure.Expressions;
using Remotion.Linq.Clauses.Expressions;

namespace MongoDB.Driver.Relinq.Preparation
{
    internal class PreparingExpressionVisitor : MongoExpressionVisitor
    {
        public static Expression Prepare(Expression expression, IPipelinePreparationContext context)
        {
            var visitor = new PreparingExpressionVisitor(context);
            return visitor.Visit(expression);
        }

        private readonly IPipelinePreparationContext _context;

        public PreparingExpressionVisitor(IPipelinePreparationContext context)
        {
            _context = Ensure.IsNotNull(context, "context");
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            Expression replacement;
            if (_context.TryGetExpressionMapping(node, out replacement))
            {
                node = replacement;
            }

            return base.Visit(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var newNode = base.VisitMember(node);
            var newMemberNode = newNode as MemberExpression;
            if (newMemberNode != null)
            {
                var serializationExpression = newMemberNode.Expression as SerializationExpression;
                if (serializationExpression != null)
                {
                    var bsonDocumentSerializer = serializationExpression.Serializer as IBsonDocumentSerializer;
                    BsonSerializationInfo info;
                    if (bsonDocumentSerializer != null &&
                        bsonDocumentSerializer.TryGetMemberSerializationInfo(newMemberNode.Member.Name, out info))
                    {
                        var fieldExpression = serializationExpression as IFieldExpression;
                        return new FieldExpression(
                            fieldExpression.AppendFieldName(info.ElementName),
                            info.Serializer);
                    }
                }
            }

            return newNode;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            return EmbeddedPipelineBuildingQueryModelVisitor.Prepare(expression.QueryModel, _context);
        }
    }
}
