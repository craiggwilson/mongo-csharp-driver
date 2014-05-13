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

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// The IQueryable implementation for Linq to Aggregation.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    public class LinqToMongoQueryable<TElement> : IOrderedQueryable<TElement>
    {
        // private fields
        private readonly Expression _expression;
        private readonly LinqToMongoQueryProvider _queryProvider;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqToMongoQueryable{TElement}" /> class.
        /// </summary>
        /// <param name="queryProvider">The query provider.</param>
        /// <exception cref="System.ArgumentNullException">queryProvider</exception>
        public LinqToMongoQueryable(LinqToMongoQueryProvider queryProvider)
        {
            if (queryProvider == null)
            {
                throw new ArgumentNullException("queryProvider");
            }

            _queryProvider = queryProvider;
            _expression = Expression.Constant(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinqToMongoQueryable{TElement}" /> class.
        /// </summary>
        /// <param name="queryProvider">The query provider.</param>
        /// <param name="expression">The expression.</param>
        /// <exception cref="System.ArgumentNullException">queryProvider</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">expression</exception>
        public LinqToMongoQueryable(LinqToMongoQueryProvider queryProvider, Expression expression)
        {
            if (queryProvider == null)
            {
                throw new ArgumentNullException("queryProvider");
            }
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            else if (!typeof(IQueryable<TElement>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentOutOfRangeException("expression");
            }

            _expression = expression;
            _queryProvider = queryProvider;
        }

        // public properties

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable" /> is executed.
        /// </summary>
        /// <returns>A <see cref="T:System.Type" /> that represents the type of the element(s) that are returned when the expression tree associated with this object is executed.</returns>
        public Type ElementType
        {
            get { return typeof(TElement); }
        }

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.
        /// </summary>
        /// <returns>The <see cref="T:System.Linq.Expressions.Expression" /> that is associated with this instance of <see cref="T:System.Linq.IQueryable" />.</returns>
        public Expression Expression
        {
            get { return _expression; }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <returns>The <see cref="T:System.Linq.IQueryProvider" /> that is associated with this data source.</returns>
        public IQueryProvider Provider
        {
            get { return _queryProvider; }
        }

        // public methods
        /// <summary>
        /// Builds the query model.
        /// </summary>
        /// <returns>The query model.</returns>
        public ExecutionModel BuildQueryModel()
        {
            return _queryProvider.BuildQueryModel(Expression);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            return ((IEnumerable<TElement>)_queryProvider.Execute(_expression)).GetEnumerator();
        }

        // explicit implementations
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_queryProvider.Execute(_expression)).GetEnumerator();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return BuildQueryModel().ToString();
        }
    }
}