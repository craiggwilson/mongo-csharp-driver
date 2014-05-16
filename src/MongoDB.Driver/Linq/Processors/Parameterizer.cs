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
using System.Linq;
using System.Linq.Expressions;

namespace MongoDB.Driver.Linq
{
    /// <remarks>
    /// We find all the constant expressions and replace them with a parameter.  This allows
    /// us to later use different values because we determine that the two trees are equal
    /// when the parameter names and types are the same (see ExpressionComparer).  This does
    /// not work with indexed operations (array index, get_Item method) because the index
    /// is encoded as part of the element name in MongoDB... { "a.0" : 10} where 0 is the index.
    /// </remarks>
    internal class Parameterizer : MongoDB.Driver.Linq.Expressions.ExpressionVisitor
    {
        private readonly List<ParameterExpression> _parameters;
        private readonly List<object> _values;

        public Parameterizer()
        {
            _parameters = new List<ParameterExpression>();
            _values = new List<object>();
        }

        public Expression Parameterize(Expression node, out ParameterExpression[] parameters, out object[] values)
        {
            var result = Visit(node);
            parameters = _parameters.ToArray();
            values = _values.ToArray();
            return result;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                // ends up getting encoded as an element name, 
                // which cannot be parameterized
                return node;
            }

            return base.VisitBinary(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "get_Item" && node.Arguments.Count == 1)
            {
                // ends up getting encoded as an element name, 
                // which cannot be parameterized
                return node;
            }

            if (node.Method.Name == "ElementAt" && node.Arguments.Count == 2)
            {
                // ends up getting encoded as an element name, 
                // which cannot be parameterized
                return node;
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            bool isQueryRoot = node.Value is IQueryable;

            if (!isQueryRoot && !CanBeParameter(node))
            {
                return node;
            }

            var parameter = Expression.Parameter(node.Type, "p" + _parameters.Count);
            _parameters.Add(parameter);
            _values.Add(node.Value);

            return parameter;
        }

        private bool CanBeParameter(ConstantExpression c)
        {
            return true;
        }
    }
}