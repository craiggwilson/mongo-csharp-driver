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
    internal class MatchReducer : LinqToMongoExpressionVisitor
    {
        // public methods
        public Expression Reduce(Expression node)
        {
            return Visit(node);
        }

        // protected methods
        protected override Expression VisitMatch(MatchExpression node)
        {
            var source = Visit(node.Source);

            var sourceAsMatch = source as MatchExpression;
            if (sourceAsMatch != null)
            {
                var match = Expression.AndAlso(
                    sourceAsMatch.Match,
                    node.Match);

                return new MatchExpression(sourceAsMatch.Source, match);
            }

            if (node.Source != source)
            {
                return new MatchExpression(source, node.Match);
            }

            return node;
        }
    }
}