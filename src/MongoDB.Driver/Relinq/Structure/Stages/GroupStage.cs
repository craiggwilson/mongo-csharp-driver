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

using System.Linq.Expressions;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Relinq.Structure.Stages
{
    internal class GroupStage : PipelineStage
    {
        private readonly Expression _idSelector;
        private readonly Expression _elementSelector;

        public GroupStage(Expression idSelector)
            : this(idSelector, null)
        {
        }

        public GroupStage(Expression idSelector, Expression elementSelector)
        {
            _idSelector = Ensure.IsNotNull(idSelector, "idSelector");
            _elementSelector = elementSelector;
        }

        public Expression ElementSelector
        {
            get { return _elementSelector; }
        }

        public Expression IdSelector
        {
            get { return _idSelector; }
        }

        public override PipelineStageType StageType
        {
            get { return PipelineStageType.Group; }
        }
    }
}
