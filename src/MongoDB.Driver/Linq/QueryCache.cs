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
using MongoDB.Driver.Linq.Processors;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Handles caching the expensive part of query generation.
    /// </summary>
    internal class QueryCache
    {
        private LeastRecentlyUsedCache<CachedQuery> _cache;

        public QueryCache(int maxSize)
        {
            _cache = new LeastRecentlyUsedCache<CachedQuery>(
                maxSize,
                (a, b) => new ExpressionComparer().Compare(a.Original, b.Original));
        }

        public Expression Find(Expression node, ConstantExpression queryable)
        {
            // makes a VB expression tree look like a C# expression tree.
            // also does things like make the constant in a comparison expression
            // always be on the right side.
            node = new ExpressionNormalizer().Normalize(node);

            CachedQuery query;
            object[] values;
            if (_cache.MaxSize == 0)
            {
                // If we aren't caching anything, then we don't need to do any 
                // parameterization or parameter replacement.
                query = new CachedQuery(node, new ParameterExpression[0]);
                values = new object[0];
            }
            else
            {
                ParameterExpression[] parameters;
                node = new Parameterizer().Parameterize(node, out parameters, out values);
                query = new CachedQuery(node, parameters);

                query = _cache.GetOrAdd(query);
            }

            return query.Prepare(queryable, values);
        }
    }
}