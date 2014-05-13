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
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Driver.Linq.Processors.PipelineOperationBinders;

namespace MongoDB.Driver.Linq.Translators
{
    /// <summary>
    /// Translates a ProjectExpression into a $project clause.
    /// </summary>
    internal class PipelineProjectTranslator : LinqToMongoExpressionVisitor
    {
        // private fields
        // we sometimes just want to include a field and not rename it...
        // we can force ourselves to not use field includsion semantics
        // by setting this to false.
        private bool _useFieldInclusionSemantics;

        public PipelineProjectTranslator()
            : this(true)
        { }

        public PipelineProjectTranslator(bool useFieldInclusionSemantics)
        {
            _useFieldInclusionSemantics = useFieldInclusionSemantics;
        }

        // public methods
        /// <summary>
        /// Builds the aggregations.
        /// </summary>
        /// <param name="aggregations">The aggregations.</param>
        /// <returns>A document representing the non _id fields of a $group element.</returns>
        public BsonDocument BuildAggregations(IEnumerable<Expression> aggregations)
        {
            _useFieldInclusionSemantics = false;
            var doc = new BsonDocument();
            foreach (var aggregation in aggregations)
            {
                var field = (FieldExpression)aggregation;
                var value = ResolveValue(field.Expression);
                doc.Add(field.SerializationInfo.ElementName, value);
            }

            return doc;
        }

        /// <summary>
        /// Builds the group id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>A document for the _id field of a $group element.</returns>
        public BsonDocument BuildGroupId(Expression id)
        {
            _useFieldInclusionSemantics = false;
            var value = ResolveValue(id);

            return new BsonDocument("_id", value);
        }

        /// <summary>
        /// Builds the $project document.
        /// </summary>
        /// <param name="projector">The projector.</param>
        /// <returns>A document representing a $project document.</returns>
        public BsonDocument BuildProject(Expression projector)
        {
            var value = ResolveValue(projector);
            if (!value.IsBsonDocument)
            {
                // we are only projecting a single field, so let's remove the '$' 
                // from the beginning...
                value = new BsonDocument(value.AsString.Substring(1), 1);
            }
            else if (projector.NodeType != ExpressionType.New)
            {
                value = new BsonDocument(ProjectBinder.ScalarProjectionFieldName, value);
            }

            var doc = value.AsBsonDocument;
            if (!doc.Contains("_id"))
            {
                doc.Add("_id", 0); // we don't want the id back unless we asked for it...
            }

            return doc;
        }

        // private methods
        public BsonValue BuildValue(Expression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return BuildAdd((BinaryExpression)node);
                case (ExpressionType)LinqToMongoExpressionType.Aggregation:
                    return BuildAggregate((AggregationExpression)node);
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return BuildOperation((BinaryExpression)node, "$and", true);
                case ExpressionType.Call:
                    return BuildMethodCall((MethodCallExpression)node);
                case ExpressionType.Coalesce:
                    return BuildOperation((BinaryExpression)node, "$ifNull", false);
                case ExpressionType.Conditional:
                    return BuildConditional((ConditionalExpression)node);
                case ExpressionType.Constant:
                    return BsonValue.Create(((ConstantExpression)node).Value);
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return BuildValue(((UnaryExpression)node).Operand);
                case ExpressionType.Divide:
                    return BuildOperation((BinaryExpression)node, "$divide", false);
                case ExpressionType.Equal:
                    return BuildOperation((BinaryExpression)node, "$eq", false);
                case ExpressionType.GreaterThan:
                    return BuildOperation((BinaryExpression)node, "$gt", false);
                case ExpressionType.GreaterThanOrEqual:
                    return BuildOperation((BinaryExpression)node, "$gte", false);
                case ExpressionType.LessThan:
                    return BuildOperation((BinaryExpression)node, "$lt", false);
                case ExpressionType.LessThanOrEqual:
                    return BuildOperation((BinaryExpression)node, "$lte", false);
                case ExpressionType.MemberAccess:
                    return BuildMemberAccess((MemberExpression)node);
                case ExpressionType.Modulo:
                    return BuildOperation((BinaryExpression)node, "$mod", false);
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return BuildOperation((BinaryExpression)node, "$multiply", true);
                case ExpressionType.New:
                    return BuildNew((NewExpression)node);
                case ExpressionType.Not:
                    return BuildNot((UnaryExpression)node);
                case ExpressionType.NotEqual:
                    return BuildOperation((BinaryExpression)node, "$ne", false);
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return BuildOperation((BinaryExpression)node, "$or", true);
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return BuildOperation((BinaryExpression)node, "$subtract", false);
            }

            throw LinqErrors.UnsupportedAggregateOperator(node);
        }

        private BsonValue BuildAdd(BinaryExpression node)
        {
            var op = "$add";
            if (node.Left.Type == typeof(string))
            {
                op = "$concat";
            }

            return BuildOperation(node, op, true);
        }

        private BsonValue BuildAggregate(AggregationExpression node)
        {
            switch (node.AggregationType)
            {
                case AggregationType.Average:
                    return new BsonDocument("$avg", ResolveValue(node.Argument));
                case AggregationType.First:
                    return new BsonDocument("$first", ResolveValue(node.Argument));
                case AggregationType.Last:
                    return new BsonDocument("$last", ResolveValue(node.Argument));
                case AggregationType.Max:
                    return new BsonDocument("$max", ResolveValue(node.Argument));
                case AggregationType.Min:
                    return new BsonDocument("$min", ResolveValue(node.Argument));
                case AggregationType.Sum:
                    return new BsonDocument("$sum", ResolveValue(node.Argument));
            }

            throw LinqErrors.UnsupportedAggregateOperator(node);
        }

        private BsonValue BuildConditional(ConditionalExpression node)
        {
            var condition = ResolveValue(node.Test);
            var truePart = ResolveValue(node.IfTrue);
            var falsePart = ResolveValue(node.IfFalse);

            return new BsonDocument("$cond", new BsonArray(new[] { condition, truePart, falsePart }));
        }

        private BsonValue BuildMemberAccess(MemberExpression node)
        {
            if (node.Expression.Type == typeof(DateTime))
            {
                return BuildDateTime(node);
            }

            var message = string.Format("Members of {0} are not supported in a $project or $group node.", node.Expression.Type);
            throw new NotSupportedException(message);
        }

        private BsonValue BuildDateTime(MemberExpression node)
        {
            var field = ResolveValue(node.Expression);
            switch (node.Member.Name)
            {
                case "Day":
                    return new BsonDocument("$dayOfMonth", field);
                case "DayOfWeek":
                    return new BsonDocument("$dayOfWeek", field);
                case "DayOfYear":
                    return new BsonDocument("$dayOfYear", field);
                case "Hour":
                    return new BsonDocument("$hour", field);
                case "Millisecond":
                    return new BsonDocument("$millisecond", field);
                case "Minute":
                    return new BsonDocument("$minute", field);
                case "Month":
                    return new BsonDocument("$month", field);
                case "Second":
                    return new BsonDocument("$second", field);
                case "Year":
                    return new BsonDocument("$year", field);
            }

            throw LinqErrors.UnsupportedQueryOperator(node);
        }

        private BsonValue BuildLinqMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name != "Select")
            {
                throw LinqErrors.UnsupportedQueryOperator(node);
            }

            // we need to make sure that the serialization info for the parameter
            // is the item serialization from the parent IBsonArraySerializer
            var serializationExpression = node.Arguments[0] as IBsonSerializationInfoExpression;
            if (serializationExpression != null)
            {
                var body = ((LambdaExpression)node.Arguments[1]).Body;
                return ResolveValue(body);
            }

            throw LinqErrors.UnsupportedQueryOperator(node);
        }

        private BsonValue BuildMethodCall(MethodCallExpression node)
        {
            if (IsLinqMethod(node))
            {
                return BuildLinqMethodCall(node);
            }

            if (node.Type == typeof(string))
            {
                return BuildStringMethodCall(node);
            }

            throw LinqErrors.UnsupportedQueryOperator(node);
        }

        private BsonValue BuildNew(NewExpression node)
        {
            var useFieldInclusionSemanticsSave = _useFieldInclusionSemantics;
            _useFieldInclusionSemantics = false;
            BsonDocument doc = new BsonDocument();
            var parameters = node.Constructor.GetParameters();
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                var value = ResolveValue(node.Arguments[i]);
                var field = node.Arguments[i] as FieldExpression;
                if (field != null)
                {
                    if (field.IsProjected || !useFieldInclusionSemanticsSave)
                    {
                        doc.Add(parameters[i].Name, value);
                    }
                    else
                    {
                        doc.Add(field.SerializationInfo.ElementName, 1);
                    }
                }
                else
                {
                    doc.Add(parameters[i].Name, value);
                }
            }
            _useFieldInclusionSemantics = useFieldInclusionSemanticsSave;

            return doc;
        }

        private BsonValue BuildNot(UnaryExpression node)
        {
            var operand = ResolveValue(node.Operand);
            if (operand.IsBsonDocument)
            {
                operand = new BsonArray().Add(operand);
            }
            return new BsonDocument("$not", operand);
        }

        private BsonValue BuildOperation(BinaryExpression node, string op, bool canBeFlattened)
        {
            var left = ResolveValue(node.Left);
            var right = ResolveValue(node.Right);

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

        private BsonValue BuildStringMethodCall(MethodCallExpression node)
        {
            var field = ResolveValue(node.Object);
            switch (node.Method.Name)
            {
                case "Substring":
                    if (node.Arguments.Count == 2)
                    {
                        var start = ResolveValue(node.Arguments[0]);
                        var end = ResolveValue(node.Arguments[1]);
                        return new BsonDocument("$substr", new BsonArray(new[] { field, start, end }));
                    }
                    break;
                case "ToLower":
                case "ToLowerInvariant":
                    if (node.Arguments.Count == 0)
                    {
                        return new BsonDocument("$toLower", field);
                    }
                    break;
                case "ToUpper":
                case "ToUpperInvariant":
                    if (node.Arguments.Count == 0)
                    {
                        return new BsonDocument("$toUpper", field);
                    }
                    break;
                default:
                    throw LinqErrors.UnsupportedQueryOperator(node);
            }

            throw LinqErrors.UnsupportedQueryOperatorOverload(node);
        }

        private BsonValue ResolveValue(Expression node)
        {
            var serializationExpression = node as IBsonSerializationInfoExpression;
            if (serializationExpression != null)
            {
                return "$" + serializationExpression.SerializationInfo.ElementName;
            }

            return BuildValue(node);
        }
    }
}