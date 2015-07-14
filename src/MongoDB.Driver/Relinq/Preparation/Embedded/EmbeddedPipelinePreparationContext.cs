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
* 
*/

using System.Linq.Expressions;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Relinq.Preparation.Embedded
{
    internal class EmbeddedPipelinePreparationContext : IPipelinePreparationContext
    {
        private readonly ExpressionMapping _mapping;
        private readonly IPipelinePreparationContext _parentContext;

        public EmbeddedPipelinePreparationContext(IPipelinePreparationContext parentContext)
        {
            _parentContext = Ensure.IsNotNull(parentContext, "parentContext");
            _mapping = new ExpressionMapping();
        }

        public void AddExpressionMapping(Expression original, Expression replacement)
        {
            _mapping.AddMapping(original, replacement);
        }

        public Expression PrepareFromExpression(Expression from)
        {
            return PrepareExpression(from);
        }

        public Expression PrepareSelectExpression(Expression selector)
        {
            return PrepareExpression(selector);
        }

        public Expression PrepareWhereExpression(Expression predicate)
        {
            return PrepareExpression(predicate);
        }

        public bool TryGetExpressionMapping(Expression original, out Expression replacement)
        {
            if (_mapping.TryGetMapping(original, out replacement))
            {
                return true;
            }

            return _parentContext.TryGetExpressionMapping(original, out replacement);
        }

        private Expression PrepareExpression(Expression expression)
        {
            return PreparingExpressionVisitor.Prepare(expression, this);
        }
    }
}
