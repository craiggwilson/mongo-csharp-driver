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

using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Translators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Builds the actual expression to be executed.
    /// </summary>
    /// <remarks>
    /// Note that expressions higher in the tree than the PipelineExpression will get executed 
    /// client-side.  Therefore, we can take advantage of this fact when an operator, such as Any,
    /// relies on a supported operation to perform it's work.  It also allows the Aggregator to
    /// operate locally using linq to object expressions.
    /// </remarks>
    internal class ExecutionBuilder : LinqToMongoExpressionVisitor
    {
        // private fields
        private ExecutionTarget _executionTarget;
        private Expression _provider;

        // public methods
        /// <summary>
        /// Builds the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="executionTarget">The query target.</param>
        /// <returns>An executable expression.</returns>
        public Expression Build(Expression node, Expression provider, ExecutionTarget executionTarget)
        {
            _executionTarget = executionTarget;
            _provider = provider;
            return Visit(node);
        }

        // protected methods
        /// <summary>
        /// Visits the pipeline.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The PipelineExpression (possibly modified).</returns>
        protected override Expression VisitPipeline(PipelineExpression node)
        {
            var executionModel = new ExecutionModelBuilder().Build(node, _executionTarget);

            // this calls back into the AggQueryProvider.ExecuteQueryModel to execute
            // the actual query.
            Expression executor = Expression.Call(
                _provider,
                "ExecuteQueryModel",
                Type.EmptyTypes,
                Expression.Constant(executionModel, typeof(ExecutionModel)));

            if (executionModel.Aggregator != null)
            {
                // make sure the result is converted appropriately because the output of ExecuteQueryModel is an object.
                executor = Expression.Convert(executor, executionModel.Aggregator.Body.Type);
            }
            else
            {
                // make sure the result is converted appropriately because the output of ExecuteQueryModel is an object.
                executor = Expression.Convert(executor, typeof(IEnumerable<>).MakeGenericType(executionModel.Projection.Projector.Body.Type));
            }

            return executor;
        }
    }
}