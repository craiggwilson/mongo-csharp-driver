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
using MongoDB.Driver.Core.Misc;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Parsing;

namespace MongoDB.Driver.Relinq.Preparation
{
    internal class ComplexSelectClauseSubQueryCreatingQueryModelVisitor : QueryModelVisitorBase
    {
        public static void Apply(QueryModel queryModel)
        {
            var visitor = new ComplexSelectClauseSubQueryCreatingQueryModelVisitor();
            visitor.VisitQueryModel(queryModel);
        }

        private bool _found;
        private Expression _selector;
        private Expression _selectorReplacement;

        private ComplexSelectClauseSubQueryCreatingQueryModelVisitor()
        {
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            _selector = queryModel.SelectClause.Selector;
            base.VisitQueryModel(queryModel);

            if (_found)
            {
                var subQueryMainFromClause = new MainFromClause(
                    queryModel.GetNewName(queryModel.MainFromClause.ItemName),
                    queryModel.MainFromClause.ItemType,
                    queryModel.MainFromClause.FromExpression);
                var querySourceMapping = new QuerySourceMapping();
                querySourceMapping.AddMapping(
                    queryModel.MainFromClause,
                    new QuerySourceReferenceExpression(subQueryMainFromClause));
                var subQuerySelector = ReferenceReplacingExpressionVisitor.ReplaceClauseReferences(
                    queryModel.SelectClause.Selector,
                    querySourceMapping,
                    true);
                var subQuerySelectClause = new SelectClause(subQuerySelector);
                var newSubQueryModel = new QueryModel(
                    subQueryMainFromClause,
                    subQuerySelectClause);
                var subQueryExpression = new SubQueryExpression(newSubQueryModel);

                queryModel.MainFromClause = new MainFromClause(
                    queryModel.MainFromClause.ItemName,
                    newSubQueryModel.SelectClause.Selector.Type,
                    subQueryExpression);
                _selectorReplacement = new QuerySourceReferenceExpression(queryModel.MainFromClause);
                queryModel.SelectClause = new SelectClause(_selectorReplacement);

                base.VisitQueryModel(queryModel);
            }
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            if (_selectorReplacement != null)
            {
                whereClause.Predicate = ReplacingExpressionVisitor.Replace(
                    whereClause.Predicate,
                    _selector,
                    _selectorReplacement);
            }
            else if (ExpressionExistsExpressionVisitor.Exists(whereClause.Predicate, _selector))
            {
                _found = true;
            }
            base.VisitWhereClause(whereClause, queryModel, index);
        }

        private class ExpressionExistsExpressionVisitor : RelinqExpressionVisitor
        {
            public static bool Exists(Expression search, Expression target)
            {
                var visitor = new ExpressionExistsExpressionVisitor(target);
                visitor.Visit(search);
                return visitor._found;
            }

            private bool _found;
            private readonly Expression _target;

            private ExpressionExistsExpressionVisitor(Expression target)
            {
                _target = Ensure.IsNotNull(target, "target");
            }

            public override Expression Visit(Expression node)
            {
                if (node == _target)
                {
                    _found = true;
                    return node;
                }

                return base.Visit(node);
            }
        }

        private class ReplacingExpressionVisitor : RelinqExpressionVisitor
        {
            public static Expression Replace(Expression search, Expression target, Expression replacement)
            {
                var visitor = new ReplacingExpressionVisitor(target, replacement);
                return visitor.Visit(search);
            }

            private readonly Expression _target;
            private readonly Expression _replacement;

            private ReplacingExpressionVisitor(Expression target, Expression replacement)
            {
                _target = Ensure.IsNotNull(target, "target");
                _replacement = Ensure.IsNotNull(replacement, "replacement");
            }

            public override Expression Visit(Expression node)
            {
                if (node == _target)
                {
                    return _replacement;
                }

                return base.Visit(node);
            }
        }
    }
}