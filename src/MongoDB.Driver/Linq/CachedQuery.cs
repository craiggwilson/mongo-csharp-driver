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
using System.Text;
using System.Threading;
using MongoDB.Driver.Linq.Processors;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// A query that has been cached...
    /// </summary>
    internal class CachedQuery
    {
        // private fields
        private readonly Expression _original;
        private readonly ParameterExpression[] _parameters;
        private Expression _processed;

        // constructors
        public CachedQuery(Expression original, ParameterExpression[] parameters)
        {
            _original = original;
            _parameters = parameters;
        }

        // public properties
        /// <summary>
        /// Gets the original.
        /// </summary>
        public Expression Original
        {
            get { return _original; }
        }

        // public methods
        public Expression Prepare(ConstantExpression queryable, object[] values)
        {
            Process(queryable);

            var node = new ExpressionReplacer().ReplaceAll(_processed,
                _parameters,
                values.Select((x, i) => Expression.Constant(x, _parameters[i].Type)).ToArray());

            // we may have dropped some calculations into the expression tree
            // we need to handle...
            return PartialEvaluator.Evaluate(node);
        }

        // private methods
        private void Process(ConstantExpression queryable)
        {
            if (_processed == null)
            {
                // turn local variables and method calls into constants.
                var node = PartialEvaluator.Evaluate(_original);

                // bind expression tree patterns into pipeline concepts like $match, $sort, etc...
                node = new PipelineBinder().Bind(node);

                // certain expressions can be reordered allowing the tree to be compressed.
                node = new ExpressionReorderer().Reorder(node);

                // merge adjacent skip limits
                node = new SkipLimitReducer().Reduce(node);

                // merge adjacent sorts.
                node = new SortReducer().Reduce(node);

                // merge adjacent matches.
                node = new MatchReducer().Reduce(node);

                // rewrite aggregation operations
                node = new GroupedAggregateRewriter().Rewrite(node);

                // if a project is last, we'll drop it.
                node = new FinalProjectDropper().DropFinalProject(node);

                // we might end up doing this more than once, that is ok... 
                // they'll all end up the same
                Interlocked.Exchange(ref _processed, node);
            }
        }
    }
}