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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Translators;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// The query provider for Linq to Mongo.
    /// </summary>
    public class LinqToMongoQueryProvider : IQueryProvider
    {
        // private static fields
        private static readonly QueryCache __cache;

        // private fields
        private readonly ExecutionTarget _executionTarget;
        private readonly MongoCollection _collection;

        // constructors
        static LinqToMongoQueryProvider()
        {
            // TODO: need to figure out optimal number for this.
            // TODO: need to make this configurable
            // TODO: perhaps may way to use 0 as the default, in case this doesn't work :(
            __cache = new QueryCache(10);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinqToMongoQueryProvider" /> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="executionTarget">The query target.</param>
        public LinqToMongoQueryProvider(MongoCollection collection, ExecutionTarget executionTarget)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            _collection = collection;
            _executionTarget = executionTarget;
        }

        // public properties
        /// <summary>
        /// Gets the collection.
        /// </summary>
        public MongoCollection Collection
        {
            get { return _collection; }
        }

        // public methods
        /// <summary>
        /// Gets the query model.
        /// </summary>
        /// <param name="expression">The node.</param>
        /// <returns>A QueryModel.</returns>
        public ExecutionModel BuildQueryModel(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            var queryable = new RootQueryableFinder().Find(expression);
            var plan = __cache.Find(expression, queryable);
            return new ExecutionModelBuilder().Build(plan, _executionTarget);
        }

        /// <summary>
        /// Creates the query.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable" /> that can evaluate the query represented by the specified expression tree.
        /// </returns>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            return new LinqToMongoQueryable<TElement>(this, expression);
        }

        /// <summary>
        /// Constructs an <see cref="T:System.Linq.IQueryable" /> object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>
        /// An <see cref="T:System.Linq.IQueryable" /> that can evaluate the query represented by the specified expression tree.
        /// </returns>
        public IQueryable CreateQuery(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            Type elementType = TypeHelper.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(
                    typeof(LinqToMongoQueryable<>).MakeGenericType(elementType), new object[] { this, expression });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        /// <summary>
        /// Executes the specified node.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="expression">The node.</param>
        /// <returns>The result.</returns>
        /// <remarks>
        /// This method is used when the result of the operation
        /// is a scalar value.
        /// </remarks>
        public TResult Execute<TResult>(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            object result = Execute(expression);
            return (TResult)result;
        }

        /// <summary>
        /// Executes the specified node.
        /// </summary>
        /// <param name="expression">The node.</param>
        /// <returns>The result.</returns>
        public object Execute(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            var plan = BuildExecutionPlan(expression);

            var efn = Expression.Lambda<Func<object>>(Expression.Convert(plan, typeof(object)));
            var fn = efn.Compile();
            return fn();
        }

        // internal methods
        /// <summary>
        /// Executes the query model.
        /// </summary>
        /// <param name="executionModel">The query model.</param>
        /// <returns>The projected documents.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        internal object Execute(ExecutionModel executionModel)
        {
            return executionModel.Execute(_collection);
        }

        // private methods
        private Expression BuildExecutionPlan(Expression node)
        {
            var lambda = node as LambdaExpression;
            if (lambda != null)
            {
                node = lambda.Body;
            }

            var queryable = new RootQueryableFinder().Find(node);
            node = __cache.Find(node, queryable);

            var provider = Expression.Convert(
                Expression.Property(queryable, typeof(IQueryable).GetProperty("Provider")),
                typeof(LinqToMongoQueryProvider));

            return new ExecutionBuilder().Build(node, provider, _executionTarget);
        }
    }
}