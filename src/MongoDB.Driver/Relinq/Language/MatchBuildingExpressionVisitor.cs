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
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Relinq.Structure.Expressions;

namespace MongoDB.Driver.Relinq.Language
{
    internal class MatchBuildingExpressionVisitor
    {
        public static BsonValue Build(Expression expression)
        {
            var visitor = new MatchBuildingExpressionVisitor();
            var document = visitor.BuildFilter(expression);
            return document.Render(BsonDocumentSerializer.Instance, BsonSerializer.SerializerRegistry);
        }

        private static readonly FilterDefinitionBuilder<BsonDocument> __builder = Builders<BsonDocument>.Filter;

        private MatchBuildingExpressionVisitor()
        {
        }

        private FilterDefinition<BsonDocument> BuildFilter(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    return VisitBinary((BinaryExpression)expression);
                case ExpressionType.Extension:
                    var mongoExpression = expression as MongoExpression;
                    if (mongoExpression != null)
                    {
                        switch (mongoExpression.MongoNodeType)
                        {
                            case MongoExpressionType.AnyElementTrue:
                                return BuildAnyElementTrue((AnyElementTrueExpression)expression);
                        }
                    }
                    break;
            }

            throw new NotSupportedException();
        }

        private FilterDefinition<BsonDocument> BuildAnyElementTrue(AnyElementTrueExpression expression)
        {
            var fieldExpression = expression.Source as IFieldExpression;
            if (fieldExpression == null)
            {
                var arrayItemExpression = (ArrayItemExpression)expression.Source;
                fieldExpression = arrayItemExpression.ReferencedField;
            }

            FilterDefinition<BsonDocument> filter;
            var renderWithoutElemMatch = CanAnyBeRenderedWithoutElemMatch(expression.Predicate);

            if (renderWithoutElemMatch)
            {
                var predicate = FieldNamePrefixingExpressionVisitor.Prefix(expression.Predicate, fieldExpression.FieldName);
                filter = BuildFilter(predicate);
            }
            else
            {
                filter = __builder.ElemMatch(fieldExpression.FieldName, BuildFilter(expression.Predicate));
                if (!(fieldExpression.Serializer is IBsonDocumentSerializer))
                {
                    filter = new ScalarElementMatchFilterDefinition<BsonDocument>(filter);
                }
            }

            return filter;
        }

        private bool CanAnyBeRenderedWithoutElemMatch(Expression expression)
        {
            switch (expression.NodeType)
            {
                // this doesn't cover all cases, but absolutely covers
                // the most common ones. This is opt-in behavior, so
                // when someone else discovers an Any query that shouldn't
                // be rendered with $elemMatch, we'll have to add it in.
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    return true;
                case ExpressionType.Call:
                    var callNode = (MethodCallExpression)expression;
                    switch (callNode.Method.Name)
                    {
                        case "StartsWith":
                        case "EndsWith":
                            return true;
                        default:
                            return false;
                    }
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Not:
                    var unaryExpression = (UnaryExpression)expression;
                    return CanAnyBeRenderedWithoutElemMatch(unaryExpression.Operand);
                default:
                    return false;
            }
        }

        private FilterDefinition<BsonDocument> VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.Equal)
            {
                var field = (FieldExpression)node.Left;
                var constant = (ConstantExpression)node.Right;

                return __builder.Eq(field.FieldName, constant.Value);
            }

            throw new NotSupportedException();
        }
    }
}
