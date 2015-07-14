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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Relinq.Structure.Expressions;

namespace MongoDB.Driver.Relinq.Language
{
    internal class FieldNamePrefixingExpressionVisitor : MongoExpressionVisitor
    {
        public static Expression Prefix(Expression node, string prefix)
        {
            var replacer = new FieldNamePrefixingExpressionVisitor(prefix);
            return replacer.Visit(node);
        }

        private readonly string _prefix;

        private FieldNamePrefixingExpressionVisitor(string prefix)
        {
            _prefix = prefix;
        }

        protected internal override Expression VisitField(FieldExpression expression)
        {
            return new FieldExpression(
                expression.PrependFieldName(_prefix),
                expression.Serializer);
        }
    }
}
