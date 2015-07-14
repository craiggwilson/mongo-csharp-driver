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
    internal class MatchStage : PipelineStage
    {
        private readonly Expression _predicate;

        public MatchStage(Expression predicate)
        {
            _predicate = Ensure.IsNotNull(predicate, "predicate");
        }

        public override PipelineStageType StageType
        {
            get { return PipelineStageType.Match; }
        }

        public Expression Predicate
        {
            get { return _predicate; }
        }
    }
}
