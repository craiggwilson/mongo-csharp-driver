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
using System.Reflection;
using System.Text;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.PipelineOperationBinders
{
    /// <summary>
    /// Binds aggregates that occur at the root of a tree.
    /// </summary>
    internal class RootAggregateBinder : ProjectingPipelineOperationBinder
    {
        public RootAggregateBinder(Dictionary<Expression, GroupExpression> groupMap)
            : base(groupMap)
        { }

        // public methods
        /// <summary>
        /// Binds a root aggregate operation.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="argument">The argument.</param>
        /// <param name="rootAggregationType">Type of the root aggregation.</param>
        /// <returns></returns>
        public Expression Bind(MethodInfo method, PipelineExpression pipeline, LambdaExpression argument, RootAggregationType rootAggregationType)
        {
            string aggregatorMethodName;
            switch(rootAggregationType)
            {
                case RootAggregationType.Count:
                    aggregatorMethodName = "SingleOrDefault";
                    break;
                default:
                    aggregatorMethodName = "Single";
                    break;
            }

            var parameter = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(method.ReturnType), "p");
            var aggregator = Expression.Lambda(
                Expression.Call(typeof(Enumerable), aggregatorMethodName, new[] { method.ReturnType }, parameter),
                parameter);

            RegisterProjector(pipeline.Projector);

            var source = pipeline.Source;
            Expression argExpression;
            switch(rootAggregationType)
            {
                case RootAggregationType.Count:
                    argExpression = Expression.Constant(1);
                    break;
                default:
                    if (argument != null)
                    {
                        RegisterParameterReplacement(argument.Parameters[0], pipeline.Projector);
                        argExpression = Visit(argument.Body);
                    }
                    else
                    {
                        argExpression = pipeline.Projector;
                        var project = source as ProjectExpression;
                        if (project != null)
                        {
                            // eliminate the project from the pipeline since
                            // it is now contained in the argExpression.
                            source = project.Source;
                        }
                    }
                    break;
            }

            AggregationType aggregationType;
            IBsonSerializer serializer;
            switch(rootAggregationType)
            {
                case RootAggregationType.Average:
                    aggregationType = AggregationType.Average;
                    serializer = LookupSerializer(method.ReturnType, argExpression);
                    break;
                case RootAggregationType.Count:
                case RootAggregationType.Sum:
                    aggregationType = AggregationType.Sum;
                    serializer = BsonSerializer.LookupSerializer(method.ReturnType);
                    break;
                case RootAggregationType.Min:
                    aggregationType = AggregationType.Min;
                    serializer = LookupSerializer(method.ReturnType, argExpression);
                    break;
                case RootAggregationType.Max:
                    aggregationType = AggregationType.Max;
                    serializer = LookupSerializer(method.ReturnType, argExpression);
                    break;
                default:
                    throw new InvalidOperationException("Unknown RootAggregationType.");
            }

            var aggregationExpression = new FieldExpression(
                new AggregationExpression(method.ReturnType, aggregationType, argExpression),
                new BsonSerializationInfo(
                    "_agg0",
                    serializer,
                    method.ReturnType,
                    serializer.GetDefaultSerializationOptions()),
                true);

            return new PipelineExpression(
                new RootAggregationExpression(source, aggregationExpression, rootAggregationType),
                aggregationExpression,
                aggregator);
        }

        /// <summary>
        /// Binds an ElementAt operation.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public Expression BindElementAt(PipelineExpression result, Expression index)
        {
            // skip the index and take 1 and then run the method locally against
            // the result set to get the same semantics as Linq to Objects.
            return BindSkipLimitWithAggregator(result, index, Expression.Constant(1), "ElementAt", Expression.Constant(0));
        }

        /// <summary>
        /// Binds an ElementAt operation.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public Expression BindElementAtOrDefault(PipelineExpression result, Expression index)
        {
            // skip the index and take 1 and then run the method locally against
            // the result set to get the same semantics as Linq to Objects.
            return BindSkipLimitWithAggregator(result, index, Expression.Constant(1), "ElementAtOrDefault", Expression.Constant(0));
        }

        /// <summary>
        /// Binds a First operation.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public Expression BindFirst(PipelineExpression result)
        {
            // take 1 and then run the method locally against
            // the result set to get the same semantics as Linq to Objects.
            return BindSkipLimitWithAggregator(result, Expression.Constant(0), Expression.Constant(1), "First");
        }

        /// <summary>
        /// Binds a FirstOrDefault operation.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public Expression BindFirstOrDefault(PipelineExpression result)
        {
            // take 1 and then run the method locally against
            // the result set to get the same semantics as Linq to Objects.
            return BindSkipLimitWithAggregator(result, Expression.Constant(0), Expression.Constant(1), "FirstOrDefault");
        }

        /// <summary>
        /// Binds a Last operation.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public Expression BindLast(PipelineExpression result)
        {
            return BindLast(result, "Last");
        }

        /// <summary>
        /// Binds a LastOrDefault operation.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public Expression BindLastOrDefault(PipelineExpression result)
        {
            return BindLast(result, "LastOrDefault");
        }

        /// <summary>
        /// Binds a Single operation.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public Expression BindSingle(PipelineExpression result)
        {
            // take 2 and then run the method locally against
            // the result set to get the same semantics as Linq to Objects.
            return BindSkipLimitWithAggregator(result, Expression.Constant(0), Expression.Constant(2), "Single");
        }

        /// <summary>
        /// Binds a SingleOrDefault operation.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public Expression BindSingleOrDefault(PipelineExpression result)
        {
            // take 2 and then run the method locally against
            // the result set to get the same semantics as Linq to Objects.
            return BindSkipLimitWithAggregator(result, Expression.Constant(0), Expression.Constant(2), "SingleOrDefault");
        }

        // private methods
        private Expression BindLast(PipelineExpression result, string methodName)
        {
            // TODO: this is horrible, but not much we can really do.  We basically bring back the whole
            // set and then take the last one locally.  Hence, last run without any filtering will
            // bring back every single document.  This is supported in the agg framework and will
            // definitely need to get run through that always.

            var documentType = result.Projector.Type;
            var parameter = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(documentType), "results");
            var aggregator = Expression.Lambda(
                Expression.Call(typeof(Enumerable), methodName, new[] { documentType }, parameter),
                parameter);

            return new PipelineExpression(
                result.Source,
                result.Projector,
                aggregator);
        }

        private Expression BindSkipLimitWithAggregator(PipelineExpression result, Expression skip, Expression limit, string aggregatorName, params Expression[] aggregatorArguments)
        {
            var source = new SkipLimitExpression(result.Source, skip, limit);

            var documentType = result.Projector.Type;
            var parameter = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(documentType), "results");
            var arguments = new List<Expression>();
            arguments.Add(parameter);
            arguments.AddRange(aggregatorArguments);
            var aggregator = Expression.Lambda(
                Expression.Call(typeof(Enumerable), aggregatorName, new[] { documentType }, arguments.ToArray()),
                parameter);

            return new PipelineExpression(
                source,
                result.Projector,
                aggregator);
        }

        private IBsonSerializer LookupSerializer(Type returnType, Expression argExpression)
        {
            if (returnType == argExpression.Type)
            {
                return LookupSerializer(argExpression);
            }

            return BsonSerializer.LookupSerializer(returnType);
        }
    }
}