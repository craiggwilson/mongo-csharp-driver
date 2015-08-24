using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors;
using MongoDB.Driver.Linq.Processors.Pipeline;
using MongoDB.Driver.Linq.Translators;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CompiledQuery
    {
        /// <summary>
        /// Compiles the specified collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static Func<TResult> Compile<T, TResult>(
            IMongoCollection<T> collection,
            Expression<Func<IMongoQueryable<T>, TResult>> query)
        {
            return new CompiledQuery(
                collection.AsQueryable(),
                query,
                collection.DocumentSerializer,
                collection.Settings.SerializerRegistry)
                .Invoke<TResult>;
        }

        /// <summary>
        /// Compiles the specified collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TArg0">The type of the arg0.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static Func<TArg0, TResult> Compile<T, TArg0, TResult>(
            IMongoCollection<T> collection,
            Expression<Func<IMongoQueryable<T>, TArg0, TResult>> query)
        {
            return new CompiledQuery(
                collection.AsQueryable(),
                query,
                collection.DocumentSerializer,
                collection.Settings.SerializerRegistry)
                .Invoke<TArg0, TResult>;
        }

        /// <summary>
        /// Compiles the specified collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TArg0">The type of the arg0.</typeparam>
        /// <typeparam name="TArg1">The type of the arg1.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static Func<TArg0, TArg1, TResult> Compile<T, TArg0, TArg1, TResult>(
            IMongoCollection<T> collection,
            Expression<Func<IMongoQueryable<T>, TArg0, TArg1, TResult>> query)
        {
            return new CompiledQuery(
                collection.AsQueryable(),
                query,
                collection.DocumentSerializer,
                collection.Settings.SerializerRegistry)
                .Invoke<TArg0, TArg1, TResult>;
        }

        private static LambdaExpression PrepareQuery(IQueryable queryable, LambdaExpression lambda, IBsonSerializer serializer, IBsonSerializerRegistry serializerRegistry)
        {
            var expression = ExpressionReplacer.Replace(lambda.Body, lambda.Parameters[0], Expression.Constant(queryable));

            expression = PartialEvaluator.Evaluate(expression);
            expression = Transformer.Transform(expression);
            expression = PipelineBinder.Bind(expression, serializer, serializerRegistry);

            return Expression.Lambda(
                expression,
                lambda.Parameters.Skip(1).ToArray());
        }

        private readonly LambdaExpression _query;
        private readonly IQueryProvider _queryProvider;
        private readonly IBsonSerializerRegistry _serializerRegistry;

        private CompiledQuery(IQueryable queryable, LambdaExpression lambda, IBsonSerializer serializer, IBsonSerializerRegistry serializerRegistry)
        {
            _query = PrepareQuery(queryable, lambda, serializer, serializerRegistry);
            _queryProvider = queryable.Provider;
            _serializerRegistry = serializerRegistry;
        }

        private TResult Invoke<TResult>()
        {
            return Execute<TResult>();
        }

        private TResult Invoke<TArg0, TResult>(TArg0 arg0)
        {
            return Execute<TResult>(arg0);
        }

        private TResult Invoke<TArg0, TArg1, TResult>(TArg0 arg0, TArg1 arg1)
        {
            return Execute<TResult>(arg0, arg1);
        }

        private Task<TResult> InvokeAsync<TArg0, TArg1, TResult>(TArg0 arg0, TArg1 arg1)
        {
            return ExecuteAsync<TResult>(arg0, arg1);
        }

        private TResult Execute<TResult>(params object[] parameterValues)
        {
            var expression = PrepareExpression(parameterValues);

            var translation = QueryableTranslator.Translate(expression, _serializerRegistry);
            var executionPlan = ExecutionPlanBuilder.Build(
                Expression.Constant(_queryProvider),
                translation);

            var lambda = Expression.Lambda(executionPlan);
            return (TResult)lambda.Compile().DynamicInvoke();
        }

        private Task<TResult> ExecuteAsync<TResult>(params object[] parameterValues)
        {
            var expression = PrepareExpression(parameterValues);

            var translation = QueryableTranslator.Translate(expression, _serializerRegistry);
            var executionPlan = ExecutionPlanBuilder.BuildAsync(
                Expression.Constant(_queryProvider),
                translation,
                Expression.Constant(CancellationToken.None));

            var lambda = Expression.Lambda(executionPlan);
            return (Task<TResult>)lambda.Compile().DynamicInvoke();
        }

        private Expression PrepareExpression(object[] parameterValues)
        {
            Expression expression = _query.Body;
            if (_query.Parameters.Count == 1)
            {
                expression = ExpressionReplacer.Replace(expression, _query.Parameters[0], Expression.Constant(parameterValues[0]));
            }
            else if (_query.Parameters.Count > 1)
            {
                var replacements = new Dictionary<Expression, Expression>(_query.Parameters.Count);
                for (int i = 0; i < _query.Parameters.Count; i++)
                {
                    replacements.Add(_query.Parameters[i], Expression.Constant(parameterValues[i]));
                }

                expression = ExpressionReplacer.Replace(expression, replacements);
            }

            return expression;
        }
    }
}
