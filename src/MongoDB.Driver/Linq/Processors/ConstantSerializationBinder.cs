/* Copyright 2015 MongoDB Inc.
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
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors
{
    internal class ConstantSerializationBinder : ExtensionExpressionVisitor
    {
        public static Expression Bind(Expression node, IBindingContext bindingContext)
        {
            var visitor = new ConstantSerializationBinder(bindingContext);
            return visitor.Visit(node);
        }

        private readonly IBindingContext _bindingContext;
        private Marker _currentMarker;

        public ConstantSerializationBinder(IBindingContext bindingContext)
        {
            _bindingContext = bindingContext;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Expression left;
            Expression right;
            using (Mark(node.Left))
            {
                right = Visit(node.Right);
            }
            using (Mark(node.Right))
            {
                left = Visit(node.Left);
            }

            return node.Update(left, node.Conversion, right);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            IBsonSerializer serializer;
            if (_currentMarker == null || !_currentMarker.TryFindSerializer(node.Type, out serializer))
            {
                serializer = _bindingContext.GetSerializer(node.Type, node);
            }

            return new ConstantSerializationExpression(
                node.Value,
                node.Type,
                serializer);
        }

        private IDisposable Mark(Expression markPoint)
        {
            return new DisposableMark(this, markPoint);
        }

        private class DisposableMark : IDisposable
        {
            private readonly ConstantSerializationBinder _binder;
            private readonly Marker _oldMark;

            public DisposableMark(ConstantSerializationBinder binder, Expression markPoint)
            {
                _binder = binder;
                _oldMark = _binder._currentMarker;
                _binder._currentMarker = new Marker(markPoint);
            }

            public void Dispose()
            {
                _binder._currentMarker = _oldMark;
            }
        }

        private class Marker : ExtensionExpressionVisitor
        {
            private readonly Expression _node;
            private IBsonSerializer _found;
            private Type _searchType;

            public Marker(Expression node)
            {
                _node = node;
            }

            public bool TryFindSerializer(Type type, out IBsonSerializer serializer)
            {
                _searchType = type;
                _found = null;
                Visit(_node);
                serializer = _found;
                return _found != null;
            }

            public override Expression Visit(Expression node)
            {
                if (_found != null)
                {
                    return node;
                }

                var serializationExpression = node as ISerializationExpression;
                if (serializationExpression != null &&
                    serializationExpression.Serializer.ValueType == _searchType)
                {
                    _found = serializationExpression.Serializer;
                    return node;
                }

                return base.Visit(node);
            }
        }
    }
}
