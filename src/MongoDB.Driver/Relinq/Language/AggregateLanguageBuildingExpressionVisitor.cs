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
using MongoDB.Driver.Relinq.Structure.Expressions;

namespace MongoDB.Driver.Relinq.Language
{
    internal class AggregateLanguageBuildingExpressionVisitor
    {
        public static BsonValue Build(Expression expression)
        {
            var visitor = new AggregateLanguageBuildingExpressionVisitor();
            return visitor.BuildValue(expression);
        }

        private AggregateLanguageBuildingExpressionVisitor()
        {
        }

        private BsonValue BuildValue(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                    return BuildAdd((BinaryExpression)expression);
                case ExpressionType.Constant:
                    return BuildConstant((ConstantExpression)expression);
                case ExpressionType.Equal:
                    return BuildEqual((BinaryExpression)expression);
                case ExpressionType.Extension:
                    var mongoExpression = expression as MongoExpression;
                    if (mongoExpression != null)
                    {
                        switch (mongoExpression.MongoNodeType)
                        {
                            case MongoExpressionType.AnyElementTrue:
                                return BuildAnyElementsTrue((AnyElementTrueExpression)expression);
                            case MongoExpressionType.DocumentWrappedField:
                                return BuildDocumentWrappedField((DocumentWrappedFieldExpression)expression);
                            case MongoExpressionType.Field:
                                return BuildField((FieldExpression)expression);
                            case MongoExpressionType.Filter:
                                return BuildFilter((FilterExpression)expression);
                        }
                    }
                    break;
            }

            throw new NotSupportedException();
        }

        private BsonValue BuildAdd(BinaryExpression expression)
        {
            var op = "$add";
            if (expression.Left.Type == typeof(string))
            {
                op = "$concat";
            }

            return BuildOperation(expression, op, true);
        }

        private BsonValue BuildAnyElementsTrue(AnyElementTrueExpression expression)
        {
            var predicate = FieldNamePrefixingExpressionVisitor.Prefix(expression.Predicate, "$" + expression.ItemName);

            return new BsonDocument("$anyElementTrue", new BsonDocument("$map", new BsonDocument
            {
                { "input", Build(expression.Source) },
                { "as", expression.ItemName },
                { "in", Build(predicate) }
            }));
        }

        private BsonValue BuildConstant(ConstantExpression expression)
        {
            var value = BsonValue.Create(((ConstantExpression)expression).Value);
            var stringValue = value as BsonString;
            if (stringValue != null && stringValue.Value.StartsWith("$"))
            {
                value = new BsonDocument("$literal", value);
            }
            // TODO: there may be other instances where we should use a literal...
            // but I can't think of any yet.
            return value;
        }

        private BsonValue BuildDocumentWrappedField(DocumentWrappedFieldExpression expression)
        {
            return new BsonDocument(expression.FieldName, Build(expression.Expression));
        }

        private BsonValue BuildEqual(BinaryExpression expression)
        {
            return BuildOperation(expression, "$eq", false);
        }

        private BsonValue BuildField(FieldExpression expression)
        {
            return "$" + expression.FieldName;
        }

        private BsonValue BuildFilter(FilterExpression expression)
        {
            var predicate = FieldNamePrefixingExpressionVisitor.Prefix(expression.Predicate, "$" + expression.ItemName);

            return new BsonDocument("$filter", new BsonDocument
            {
                { "input", Build(expression.Source) },
                { "as", expression.ItemName },
                { "cond", Build(predicate) }
            });
        }

        private BsonValue BuildOperation(BinaryExpression expression, string op, bool canBeFlattened)
        {
            var left = BuildValue(expression.Left);
            var right = BuildValue(expression.Right);

            // some operations take an array as the argument.
            // we want to flatten binary values into the top-level 
            // array if they are flattenable :).
            if (canBeFlattened && left.IsBsonDocument && left.AsBsonDocument.Contains(op) && left[op].IsBsonArray)
            {
                left[op].AsBsonArray.Add(right);
                return left;
            }

            return new BsonDocument(op, new BsonArray(new[] { left, right }));
        }
    }
}
