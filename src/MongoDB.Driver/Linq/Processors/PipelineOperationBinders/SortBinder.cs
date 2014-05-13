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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MongoDB.Driver.Linq.Processors.PipelineOperationBinders
{
    /// <summary>
    /// Binds sort operations.
    /// </summary>
    internal class SortBinder : GroupAwarePipelineOperationBinder
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SortBinder" /> class.
        /// </summary>
        /// <param name="groupMap">The group map.</param>
        public SortBinder(Dictionary<Expression, GroupExpression> groupMap)
            : base(groupMap)
        { }

        // public methods
        /// <summary>
        /// Binds a Sort operation.
        /// </summary>
        /// <param name="pipeline">The pipeline.</param>
        /// <param name="sortClauses">The sort clauses.</param>
        /// <returns></returns>
        public PipelineExpression Bind(PipelineExpression pipeline, IEnumerable<SortClause> sortClauses)
        {
            RegisterProjector(pipeline.Projector);
            List<SortClause> processedSortClauses = new List<SortClause>();
            foreach (var sortClause in sortClauses)
            {
                var lambda = (LambdaExpression)sortClause.Expression;
                RegisterParameterReplacement(lambda.Parameters[0], pipeline.Projector);
                processedSortClauses.Add(new SortClause(Visit(lambda.Body), sortClause.Direction));
            }

            return new PipelineExpression(
                new SortExpression(
                    pipeline.Source,
                    processedSortClauses),
                pipeline.Projector);
        }
    }
}