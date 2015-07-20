/* Copyright 2015 MongoDB Inc.
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
using System.Text;
using System.Threading.Tasks;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace MongoDB.Driver.Relinq.Preparation.Pipeline.ResultOperatorHandlers
{
    internal class GroupResultOperatorHandler : IPipelineResultOperatorHandler
    {
        public Type SupportedResultOperatorType
        {
            get { return typeof(GroupResultOperator); }
        }

        public void Handle(ResultOperatorBase resultOperator, PipelineBuilder builder, PipelinePreparationContext context)
        {
            var groupResultOperator = (GroupResultOperator)resultOperator;

            context.PrepareGroup(groupResultOperator.KeySelector, groupResultOperator.ElementSelector);
        }
    }
}
