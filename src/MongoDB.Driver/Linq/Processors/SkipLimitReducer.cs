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

using System.Linq.Expressions;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors
{
    internal class SkipLimitReducer : LinqToMongoExpressionVisitor
    {
        // public methods
        public Expression Reduce(Expression node)
        {
            return Visit(node);
        }

        // protected methods
        protected override Expression VisitSkipLimit(SkipLimitExpression node)
        {
            var source = Visit(node.Source);

            var sourceAsSkipLimit = source as SkipLimitExpression;
            if (sourceAsSkipLimit != null)
            {
                return Merge(sourceAsSkipLimit.Source, node.Skip, sourceAsSkipLimit.Skip, node.Limit, sourceAsSkipLimit.Limit);
            }

            if (node.Source != source)
            {
                return new SkipLimitExpression(source, node.Skip, node.Limit);
            }

            return node;
        }

        // private methods
        private Expression Merge(Expression source, Expression mySkip, Expression prevSkip, Expression myLimit, Expression prevLimit)
        {
            // we are going to embed a conditional statement into the expression tree because
            // we don't know the runtime values for this.  This conditional statement will
            // get evaluated away prior to sending it to the server.
            if (prevLimit != null)
            {
                if (myLimit != null)
                {
                    myLimit = Expression.Condition(
                        Expression.GreaterThan(myLimit, prevLimit),
                        prevLimit,
                        myLimit);
                }
                else
                {
                    myLimit = prevLimit;
                }
            }

            Expression test = Expression.Constant(false);
            if (myLimit != null && mySkip != null)
            {
                test = Expression.GreaterThan(mySkip, myLimit);
                var tempSkip = Expression.Condition(
                    test,
                    Expression.Constant(0, typeof(int)),
                    mySkip);

                var tempLimit = Expression.Condition(
                    Expression.GreaterThan(mySkip, myLimit),
                    Expression.Constant(0, typeof(int)),
                    Expression.Condition(
                        Expression.GreaterThan(
                            Expression.Constant(0),
                            Expression.Subtract(myLimit, mySkip)),
                        Expression.Constant(0),
                        Expression.Subtract(myLimit, mySkip)));

                mySkip = tempSkip;
                myLimit = tempSkip;
            }

            if (prevSkip != null)
            {
                if (mySkip != null)
                {
                    mySkip = Expression.Condition(
                        test,
                        Expression.Constant(0),
                        Expression.Add(mySkip, prevSkip));
                }
                else
                {
                    mySkip = prevSkip;
                }
            }

            return new SkipLimitExpression(source, mySkip, myLimit);
        }
    }
}