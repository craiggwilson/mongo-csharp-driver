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
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Translators
{
    internal class QueryProjectionBuilder : LinqToMongoExpressionVisitor
    {
        // private fields
        private ParameterExpression _parameter;
        private HashSet<FieldExpression> _projectedFields;

        // public methods
        public ExecutionModelProjection Build(Expression projector, Type documentType)
        {
            var fields = new FieldGatherer().Gather(projector, false);
            if (fields.Count > 0)
            {
                _parameter = Expression.Parameter(typeof(IProjectionValueStore), "document");
            }
            else
            {
                // we are projecting the entire document, so no store needs to be involved.
                _parameter = Expression.Parameter(documentType, "document");
            }

            _projectedFields = new HashSet<FieldExpression>(GetUniqueFieldsByHierarchy(fields));

            var projectorBody = Visit(projector);

            return new ExecutionModelProjection
            {
                FieldSerializationInfo = _projectedFields.GroupBy(x => x.SerializationInfo.ElementName).Select(x => x.First().SerializationInfo).ToList(),
                Projector = Expression.Lambda(projectorBody, _parameter)
            };
        }

        // protected methods
        protected override Expression VisitDocument(DocumentExpression node)
        {
            // strip out the documents when building the projection because we are still projecting from a source.
            return Visit(node.Expression);
        }

        protected override Expression VisitField(FieldExpression node)
        {
            // we need to strip out all the computed fields, although none should exist at this point.
            if (node.IsProjected)
            {
                return Visit(node.Expression);
            }
            else if (!_projectedFields.Any(x => x.SerializationInfo.ElementName == node.SerializationInfo.ElementName))
            {
                return Visit(node.Expression);
            }

            return Expression.Call(
                _parameter,
                "GetValue",
                new[] { node.Type },
                Expression.Constant(node.SerializationInfo.ElementName),
                Expression.Constant(GetDefault(node.SerializationInfo.NominalType), typeof(object)));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!IsLinqMethod(node, "Select"))
            {
                return base.VisitMethodCall(node);
            }

            var source = node.Arguments[0] as FieldExpression;
            if (source != null && !_projectedFields.Contains(source))
            {
                // We are projecting off an embedded array, but we have selected the entire
                // array and not just values within it.
                var selector = (LambdaExpression)Visit((LambdaExpression)node.Arguments[1]);
                var nestedValueStoreParameter = Expression.Parameter(_parameter.Type, selector.Parameters[0].Name);
                var newSelectorBody = new ProjectionValueStoreFieldReplacer().Replace(selector.Body, source.SerializationInfo.ElementName, nestedValueStoreParameter);

                var newSelector = Expression.Lambda(
                    Expression.GetFuncType(nestedValueStoreParameter.Type, newSelectorBody.Type),
                    newSelectorBody,
                    nestedValueStoreParameter);

                var newSourceType = typeof(IEnumerable<>).MakeGenericType(nestedValueStoreParameter.Type);
                var newSource = Expression.Call(
                    _parameter,
                    "GetValue",
                    new [] { newSourceType },
                    Expression.Constant(source.SerializationInfo.ElementName),
                    Expression.Constant(GetDefault(newSourceType), typeof(object)));

                var method = node.Method.GetGenericMethodDefinition().MakeGenericMethod(nestedValueStoreParameter.Type, newSelectorBody.Type);
                return Expression.Call(
                    null,
                    method,
                    newSource,
                    newSelector);
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // all parameters left in the tree will always be the store or the document.
            return _parameter;
        }

        // private methods
        private IEnumerable<FieldExpression> GetUniqueFieldsByHierarchy(IEnumerable<FieldExpression> usedFields)
        {
            // we want to leave out subelements when the parent element exists
            // for instance, if we have asked for both "d" and "d.e", we only want to send { "d" : 1 } to the server
            // 1) group all the used fields by their element name.
            // 2) order them by their element name in ascending order
            // 3) if any groups are prefixed by the current groups element, then skip it.

            var uniqueFields = new List<FieldExpression>();
            var skippedFields = new List<string>();
            var referenceGroups = new Queue<IGrouping<string, FieldExpression>>(usedFields.GroupBy(x => x.SerializationInfo.ElementName).OrderBy(x => x.Key));
            while (referenceGroups.Count > 0)
            {
                var referenceGroup = referenceGroups.Dequeue();
                if (!skippedFields.Contains(referenceGroup.Key))
                {
                    var hierarchicalReferenceGroups = referenceGroups.Where(x => x.Key.StartsWith(referenceGroup.Key));
                    uniqueFields.AddRange(referenceGroup);
                    skippedFields.AddRange(hierarchicalReferenceGroups.Select(x => x.Key));
                }
            }

            return uniqueFields;
        }

        private object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        // nested classes
        /// <summary>
        /// This guy is going to replace calls like store.GetValue("d.y") with nestedStore.GetValue("y").
        /// </summary>
        private class ProjectionValueStoreFieldReplacer : LinqToMongoExpressionVisitor
        {
            private string _keyPrefix;
            private Expression _source;

            public Expression Replace(Expression node, string keyPrefix, Expression source)
            {
                _keyPrefix = keyPrefix;
                _source = source;
                return Visit(node);
            }

            protected override Expression Visit(Expression node)
            {
                var methodCallNode = node as MethodCallExpression;
                if (methodCallNode == null)
                {
                    return base.Visit(node);
                }

                if (methodCallNode.Object == null || methodCallNode.Object.Type != typeof(IProjectionValueStore))
                {
                    return base.Visit(node);
                }

                var currentKey = (string)((ConstantExpression)methodCallNode.Arguments[0]).Value;

                if (!currentKey.StartsWith(_keyPrefix))
                {
                    return base.Visit(node);
                }

                var newElementName = currentKey;
                if (currentKey.Length > _keyPrefix.Length)
                {
                    newElementName = currentKey.Remove(0, _keyPrefix.Length + 1);
                }

                var defaultValue = ((ConstantExpression)methodCallNode.Arguments[1]).Value;
                return Expression.Call(
                    _source,
                    "GetValue",
                    new[] { node.Type },
                    Expression.Constant(newElementName),
                    Expression.Constant(defaultValue, typeof(object)));
            }
        }
    }
}