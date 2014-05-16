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
using MongoDB.Bson;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// This static class holds methods that can be used to express MongoDB specific query operations in LINQ queries.
    /// </summary>
    public static class LinqToMongo
    {
        /// <summary>
        /// Determines whether a sequence contains all of the specified values.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence in which to locate the values.</param>
        /// <param name="values">The values to locate in the sequence.</param>
        /// <returns>True if the sequence contains all of the specified values.</returns>
        public static bool ContainsAll<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> values)
        {
            return values.All(v => source.Contains(v));
        }

        /// <summary>
        /// Determines whether a sequence contains any of the specified values.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence in which to locate the values.</param>
        /// <param name="values">The values to locate in the sequence.</param>
        /// <returns>True if the sequence contains any of the specified values.</returns>
        public static bool ContainsAny<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> values)
        {
            return source.Any(s => values.Contains(s));
        }

        /// <summary>
        /// Returns an explanation of how the query was executed (instead of the results).
        /// </summary>
        /// <param name="source">The LINQ query to explain</param>
        /// <returns>An explanation of thow the query was executed.</returns>
        public static BsonDocument Explain<T>(this IQueryable<T> source)
        {
            return Explain<T>(source, false);
        }

        /// <summary>
        /// Returns an explanation of how the query was executed (instead of the results).
        /// </summary>
        /// <param name="source">The LINQ query to explain</param>
        /// <param name="verbose">Whether the explanation should contain more details.</param>
        /// <returns>An explanation of thow the query was executed.</returns>
        public static BsonDocument Explain<T>(this IQueryable<T> source, bool verbose)
        {
            var provider = source.Provider as LinqToMongoQueryProvider;
            if (provider == null)
            {
                throw new NotSupportedException("Explain can only be called on a Linq to Mongo queryable.");
            }

            var model = provider.BuildQueryModel(source.Expression);
            if (model.ModelType == ExecutionModelType.Query)
            {
                var queryModel = (QueryModel)model;
                if (queryModel.NumberToLimit.HasValue && queryModel.NumberToLimit.Value == 0)
                {
                    throw new MongoLinqException("A query that has a .Take(0) expression will not be sent to the server and can't be explained");
                }

                if (queryModel.IsDistinct)
                {
                    throw new MongoLinqException("Explain cannot be called when Distinct is used.");
                }

                var query = queryModel.Query ?? new QueryDocument();
                var cursor = provider.Collection.FindAs<BsonDocument>(query);
                if(queryModel.NumberToLimit != null)
                {
                    cursor.SetLimit(queryModel.NumberToLimit.Value);
                }
                if(queryModel.NumberToSkip != null)
                {
                    cursor.SetSkip(queryModel.NumberToSkip.Value);
                }
                if(queryModel.SortBy != null)
                {
                    cursor.SetSortOrder(queryModel.SortBy);
                }

                return cursor.Explain(verbose);
            }
            else
            {
                // verbosity is ignored for pipelines...
                var pipelineModel = (PipelineModel)model;

                var args = new AggregateArgs
                {
                    Pipeline = pipelineModel.Pipeline
                };
                return provider.Collection.AggregateExplain(args).Response;
            }
        }

        /// <summary>
        /// Determines whether a specified value is contained in a sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <param name="source">A sequence in which to locate the values.</param>
        /// <returns>True if the value is contained in the sequence.</returns>
        public static bool In<TSource>(this TSource value, IEnumerable<TSource> source)
        {
            return source.Contains(value);
        }

        /// <summary>
        /// Injects a low level IMongoQuery into a LINQ where clause. Can only be used in LINQ queries.
        /// </summary>
        /// <param name="query">The low level query.</param>
        /// <returns>Throws an InvalidOperationException if called.</returns>
        public static bool Inject(this IMongoQuery query)
        {
            throw new InvalidOperationException("The LinqToMongo.Inject method is only intended to be used in LINQ Where clauses.");
        }
    }
}
