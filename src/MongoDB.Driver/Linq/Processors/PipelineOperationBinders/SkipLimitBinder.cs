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

namespace MongoDB.Driver.Linq.Processors.PipelineOperationBinders
{
    internal class SkipLimitBinder : PipelineOperationBinder
    {
        // public methods
        public Expression BindSkip(PipelineExpression pipeline, Expression skip)
        {
            return BindSkipLimit(pipeline, skip, null);
        }

        public Expression BindTake(PipelineExpression pipeline, Expression take)
        {
            return BindSkipLimit(pipeline, null, take);
        }

        // private methods
        private Expression BindSkipLimit(PipelineExpression pipeline, Expression skip, Expression limit)
        {
            return new PipelineExpression(
                new SkipLimitExpression(
                    pipeline.Source,
                    skip,
                    limit),
                pipeline.Projector);
        }
    }
}