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
* 
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Relinq.Structure
{
    internal class PipelineModel
    {
        private readonly Expression _projector;
        private readonly ReadOnlyCollection<PipelineStage> _stages;

        public PipelineModel(IEnumerable<PipelineStage> stages, Expression projector)
        {
            _stages = Ensure.IsNotNull(stages, "stages") as ReadOnlyCollection<PipelineStage>;
            if (_stages == null)
            {
                _stages = stages.ToList().AsReadOnly();
            }

            _projector = Ensure.IsNotNull(projector, "projector");
        }

        public Expression Projector
        {
            get { return _projector; }
        }

        public ReadOnlyCollection<PipelineStage> Stages
        {
            get { return _stages; }
        }
    }
}
