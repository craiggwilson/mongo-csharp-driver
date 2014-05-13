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

using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors.PipelineOperationBinders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MongoDB.Driver.Linq.Processors
{
    /// <summary>
    /// Binds expression tree concepts to Aggregation Framework specific expressions.
    /// </summary>
    internal class PipelineBinder : LinqToMongoExpressionVisitor
    {
        // private fields
        private readonly Dictionary<Expression, GroupExpression> _groupMap;
        private Stack<SortClause> _thenBys;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineBinder" /> class.
        /// </summary>
        public PipelineBinder()
        {
            _groupMap = new Dictionary<Expression, GroupExpression>();
        }

        // public methods
        /// <summary>
        /// Binds the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The bound expression tree.</returns>
        public Expression Bind(Expression node)
        {
            return Visit(node);
        }

        // protected methods
        /// <summary>
        /// Visits a ConstantExpression.
        /// </summary>
        /// <param name="node">The ConstantExpression.</param>
        /// <returns>The ConstantExpression (possibly modified).</returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(LinqToMongoQueryable<>))
            {
                // this is most likely the deepest node in the tree and represents the raw collection.
                // the projection we start with is the identity projection which doesn't mutate a 
                // document at all.
                return CreateRootExpression(node);
            }

            return node;
        }

        /// <summary>
        /// Visits a MethodCallExpression.
        /// </summary>
        /// <param name="node">The MethodCallExpression.</param>
        /// <returns>The MethodCallExpression (possibly modified).</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (IsLinqMethod(node))
            {
                switch (node.Method.Name)
                {
                    case "Any":
                        if (node.Arguments.Count == 1)
                        {
                            return BindAny(node.Method, node.Arguments[0], null);
                        }
                        else if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindAny(node.Method, node.Arguments[0], GetLambda(node.Arguments[1]));
                        }
                        break;
                    case "Count":
                    case "LongCount":
                        if (node.Arguments.Count == 1)
                        {
                            return BindCount(node.Method, node.Arguments[0], null);
                        }
                        else if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindCount(node.Method, node.Arguments[0], GetLambda(node.Arguments[1]));
                        }
                        break;
                    case "Distinct":
                        if (node.Arguments.Count == 1)
                        {
                            return BindDistinct(node.Arguments[0]);
                        }
                        break;
                    case "ElementAt":
                    case "ElementAtOrDefault":
                        if (node.Arguments.Count == 2)
                        {
                            return BindElementAt(node.Method.Name, node.Arguments[0], node.Arguments[1]);
                        }
                        break;
                    case "First":
                    case "FirstOrDefault":
                        if (node.Arguments.Count == 1)
                        {
                            return BindFirstOrSingle(node.Method.Name, node.Arguments[0], null);
                        }
                        else if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindFirstOrSingle(node.Method.Name, node.Arguments[0], GetLambda(node.Arguments[1]));
                        }
                        break;
                    case "GroupBy":
                        if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindGroupBy(node.Arguments[0], GetLambda(node.Arguments[1]));
                        }
                        break;
                    case "Average":
                    case "Max":
                    case "Min":
                    case "Sum":
                        if (node.Arguments.Count == 1)
                        {
                            return BindProjectableRootAggregate(node.Method, node.Arguments[0], null);

                        }
                        else if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindProjectableRootAggregate(node.Method, node.Arguments[0], GetLambda(node.Arguments[1]));
                        }
                        break;
                    case "Last":
                    case "LastOrDefault":
                        if (node.Arguments.Count == 1)
                        {
                            return BindLast(node.Method.Name, node.Arguments[0], null);
                        }
                        else if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindLast(node.Method.Name, node.Arguments[0], GetLambda(node.Arguments[1]));
                        }
                        break;
                    case "OfType":
                        if (node.Arguments.Count == 1)
                        {
                            return BindOfType(node.Arguments[0], node.Method.GetGenericArguments()[0]);
                        }
                        break;
                    case "OrderBy":
                        if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindOrderBy(node.Arguments[0], GetLambda(node.Arguments[1]), SortDirection.Ascending);
                        }
                        break;
                    case "OrderByDescending":
                        if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindOrderBy(node.Arguments[0], GetLambda(node.Arguments[1]), SortDirection.Descending);
                        }
                        break;
                    case "Select":
                        if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindSelect(node.Arguments[0], GetLambda(node.Arguments[1]));
                        }
                        break;
                    case "Single":
                    case "SingleOrDefault":
                        if (node.Arguments.Count == 1)
                        {
                            return BindFirstOrSingle(node.Method.Name, node.Arguments[0], null);
                        }
                        else if (node.Arguments.Count == 2)
                        {
                            return BindFirstOrSingle(node.Method.Name, node.Arguments[0], GetLambda(node.Arguments[1]));
                        }
                        break;
                    case "Skip":
                        if (node.Arguments.Count == 2)
                        {
                            return BindSkip(node.Arguments[0], node.Arguments[1]);
                        }
                        break;
                    case "Take":
                        if (node.Arguments.Count == 2)
                        {
                            return BindTake(node.Arguments[0], node.Arguments[1]);
                        }
                        break;
                    case "ThenBy":
                        if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindThenBy(node.Arguments[0], GetLambda(node.Arguments[1]), SortDirection.Ascending);
                        }
                        break;
                    case "ThenByDescending":
                        if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindThenBy(node.Arguments[0], GetLambda(node.Arguments[1]), SortDirection.Descending);
                        }
                        break;
                    case "Where":
                        if (node.Arguments.Count == 2 && IsLambda(node.Arguments[1], 1))
                        {
                            return BindWhere(node.Arguments[0], (LambdaExpression)StripQuotes(node.Arguments[1]));
                        }
                        break;
                    default:
                        throw LinqErrors.UnsupportedQueryOperator(node);
                }

                throw LinqErrors.UnsupportedQueryOperatorOverload(node);
            }

            // still need to process any other method call so that parameter replacements get propogated
            // to all nodes.
            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// Visits a ParameterExpression.
        /// </summary>
        /// <param name="node">The ParameterExpression.</param>
        /// <returns>The ParameterExpression (possibly modified).</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(LinqToMongoQueryable<>))
            {
                return CreateRootExpression(node);
            }

            return base.VisitParameter(node);
        }

        // private methods
        private Expression BindAny(MethodInfo method, Expression source, LambdaExpression predicate)
        {
            // Any gets modelled in the agg framework as a $group with a { $sum : 1 } to get 
            // the count followed by a client-side evaluation of the count of whether it is 
            // greater than 0.
            // TODO: is there a better way to do this?

            method = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "Count")
                .Single(m => m.GetParameters().Length == (predicate == null ? 1 : 2))
                .GetGenericMethodDefinition().MakeGenericMethod(method.GetGenericArguments()[0]);

            var expression = BindCount(method, source, predicate);

            return Expression.GreaterThan(
                expression,
                Expression.Constant(0));
        }

        private Expression BindCount(MethodInfo method, Expression source, LambdaExpression predicate)
        {
            // count is modelled in the agg framework by summing with a grouping key of 1.
            if (predicate != null)
            {
                source = Expression.Call(typeof(Queryable), "Where", method.GetGenericArguments(), source, predicate);
            }

            var pipeline = VisitPipeline(source);
            return new RootAggregateBinder(_groupMap).Bind(method, pipeline, null, RootAggregationType.Count);
        }

        private Expression BindDistinct(Expression source)
        {
            // distinct can be achieved by using the entire projection as the _id in a $group
            // expression without any aggregations.

            var pipeline = VisitPipeline(source);
            return new GroupBinder(_groupMap).BindDistinct(pipeline);
        }

        private Expression BindElementAt(string methodName, Expression source, Expression index)
        {
            var pipeline = VisitPipeline(source);

            // ElementAt is a skip limit where we skip to the index and limit 1.  The resulting
            // client-side projection will call ElementAt(0).

            var binder = new RootAggregateBinder(_groupMap);
            if (methodName == "ElementAt")
            {
                return binder.BindElementAt(pipeline, index);
            }
            else
            {
                return binder.BindElementAtOrDefault(pipeline, index);
            }
        }

        private Expression BindFirstOrSingle(string methodName, Expression source, LambdaExpression predicate)
        {
            // NOTE: these will likely need to get converted into aggregation commands
            // and then interpreted in the QueryModelBuilder.
            if (predicate != null)
            {
                source = Expression.Call(typeof(Queryable), "Where", new[] { predicate.Parameters[0].Type }, source, predicate);
            }

            var pipeline = VisitPipeline(source);

            var binder = new RootAggregateBinder(_groupMap);
            switch (methodName)
            {
                case "First":
                    return binder.BindFirst(pipeline);
                case "FirstOrDefault":
                    return binder.BindFirstOrDefault(pipeline);
                case "Single":
                    return binder.BindSingle(pipeline);
                case "SingleOrDefault":
                    return binder.BindSingleOrDefault(pipeline);
                default:
                    throw new InvalidOperationException("BindFirstOrSingle exists in an invalid state.");
            }
        }

        private Expression BindGroupBy(Expression source, LambdaExpression keySelector)
        {
            var pipeline = VisitPipeline(source);

            return new GroupBinder(_groupMap).BindGroupBy(pipeline, keySelector);
        }

        private Expression BindLast(string methodName, Expression source, LambdaExpression predicate)
        {
            if (predicate != null)
            {
                source = Expression.Call(typeof(Queryable), "Where", new[] { predicate.Parameters[0].Type }, source, predicate);
            }

            var pipeline = VisitPipeline(source);

            return methodName == "Last" ?
                new RootAggregateBinder(_groupMap).BindLast(pipeline) :
                new RootAggregateBinder(_groupMap).BindLastOrDefault(pipeline);
        }

        private Expression BindOfType(Expression source, Type type)
        {
            var pipeline = VisitPipeline(source);

            return new MatchBinder(_groupMap).BindOfType(pipeline, type);
        }

        private Expression BindOrderBy(Expression source, LambdaExpression orderSelector, SortDirection direction)
        {
            // OrderBy is always the terminating operator and will always get visited
            // after a ThenBy.
            var pipeline = VisitPipeline(source);

            if (_thenBys == null)
            {
                _thenBys = new Stack<SortClause>();
            }
            _thenBys.Push(new SortClause(orderSelector, direction));

            pipeline = new SortBinder(_groupMap).Bind(pipeline, _thenBys);
            _thenBys = null;
            return pipeline;
        }

        private Expression BindProjectableRootAggregate(MethodInfo method, Expression source, LambdaExpression argument)
        {
            var pipeline = VisitPipeline(source);

            RootAggregationType type;
            switch(method.Name)
            {
                case "Average":
                    type = RootAggregationType.Average;
                    break;
                case "Max":
                    type = RootAggregationType.Max;
                    break;
                case "Min":
                    type = RootAggregationType.Min;
                    break;
                case "Sum":
                    type = RootAggregationType.Sum;
                    break;
                default:
                    throw LinqErrors.UnreachableCode(string.Format("{0} is not a supported method.", method.Name));
            }

            return new RootAggregateBinder(_groupMap).Bind(method, pipeline, argument, type);
        }

        private Expression BindSelect(Expression source, LambdaExpression selector)
        {
            if (selector.Parameters.Count == 2)
            {
                throw new NotSupportedException("The indexed version of the Select query operator is not supported.");
            }
            if (selector.Body == selector.Parameters[0])
            {
                // this is an identity projection and can simply be ignored...
                return Visit(source);
            }

            var pipeline = VisitPipeline(source);

            return new ProjectBinder(_groupMap).Bind(pipeline, selector);
        }

        private Expression BindSkip(Expression source, Expression skip)
        {
            var pipeline = VisitPipeline(source);

            return new SkipLimitBinder().BindSkip(pipeline, skip);
        }

        private Expression BindTake(Expression source, Expression take)
        {
            var pipeline = VisitPipeline(source);

            return new SkipLimitBinder().BindTake(pipeline, take);
        }

        private Expression BindThenBy(Expression source, LambdaExpression orderSelector, SortDirection direction)
        {
            // ThenBy can only occur after an OrderBy, so we'll simply store up the ThenBy expressions
            // and let OrderBy collect them all later.
            if (_thenBys == null)
            {
                _thenBys = new Stack<SortClause>();
            }
            _thenBys.Push(new SortClause(orderSelector, direction));
            return Visit(source);
        }

        private Expression BindWhere(Expression source, LambdaExpression predicate)
        {
            var pipeline = VisitPipeline(source);

            return new MatchBinder(_groupMap).Bind(pipeline, predicate);
        }

        private Expression CreateRootExpression(Expression node)
        {
            var documentType = node.Type.GetGenericArguments()[0];
            var collectionExpression = new CollectionExpression(documentType, node);
            var serializer = BsonSerializer.LookupSerializer(documentType);
            var info = new BsonSerializationInfo(null, serializer, documentType, serializer.GetDefaultSerializationOptions());
            return new PipelineExpression(
                new CollectionExpression(documentType, node),
                new DocumentExpression(
                    Expression.Parameter(documentType, "document"),
                    info,
                    false));
        }

        private PipelineExpression EnsurePipeline(Expression source)
        {
            var pipeline = source as PipelineExpression;
            if (pipeline == null)
            {
                throw LinqErrors.InvalidSource(source);
            }

            return pipeline;
        }

        private PipelineExpression VisitPipeline(Expression source)
        {
            return EnsurePipeline(Visit(source));
        }
    }
}