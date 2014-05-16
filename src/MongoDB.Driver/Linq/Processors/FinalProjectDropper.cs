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

namespace MongoDB.Driver.Linq.Processors
{
    /// <remarks>
    /// Drops the final project in a pipeline. This allows highly-complex projections
    /// that are incapable of being run by the server to be run by the client.
    /// </remarks>
    internal class FinalProjectDropper : LinqToMongoExpressionVisitor
    {
        // public methods
        public Expression DropFinalProject(Expression node)
        {
            return Visit(node);
        }

        //protected methods
        protected override Expression VisitPipeline(PipelineExpression node)
        {
            Expression source;
            var sourceAsProject = node.Source as ProjectExpression;

            // we aren't going to process a project if it is the last element in the 
            // pipeline.  Any projections will happen locally.  The only downside
            // could be a slight increase in the size of the pulled back document. 
            // However, the plus side is that we can literally do *anything* client-side.
            if (sourceAsProject != null)
            {
                source = Visit(sourceAsProject.Source);
                return new PipelineExpression(sourceAsProject.Source, sourceAsProject.Projector);
            }

            return node;
        }
    }
}