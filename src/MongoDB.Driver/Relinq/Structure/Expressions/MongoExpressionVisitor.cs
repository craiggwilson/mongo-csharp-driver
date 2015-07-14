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
using Remotion.Linq.Parsing;

namespace MongoDB.Driver.Relinq.Structure.Expressions
{
    internal class MongoExpressionVisitor : RelinqExpressionVisitor
    {
        protected internal virtual Expression VisitAnyElementTrue(AnyElementTrueExpression expression)
        {
            return expression.Update(
                Visit(expression.Source),
                Visit(expression.Predicate));
        }

        protected internal virtual Expression VisitArrayItem(ArrayItemExpression expression)
        {
            return expression;
        }

        protected internal virtual Expression VisitDocument(DocumentExpression expression)
        {
            return expression;
        }

        protected internal virtual Expression VisitDocumentWrappedField(DocumentWrappedFieldExpression expression)
        {
            return expression.Update(Visit(expression.Expression));
        }

        internal Expression VisitEmbeddedPipeline(FilterExpression expression)
        {
            return expression;
        }

        protected internal virtual Expression VisitField(FieldExpression expression)
        {
            return expression;
        }

        protected internal virtual Expression VisitFilter(FilterExpression expression)
        {
            return expression.Update(
                Visit(expression.Source),
                Visit(expression.Predicate));
        }
    }
}
