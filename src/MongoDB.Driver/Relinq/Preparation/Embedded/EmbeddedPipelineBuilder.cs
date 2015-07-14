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

using System.Linq.Expressions;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Relinq.Preparation.Embedded
{
    internal class EmbeddedPipelineBuilder
    {
        private string _itemName;
        private Expression _currentProjector;
        private Expression _currentExpression;

        public EmbeddedPipelineBuilder(Expression currentExpression, string itemName)
        {
            _currentExpression = Ensure.IsNotNull(currentExpression, "currentExpression");
            _itemName = Ensure.IsNotNull(itemName, "itemName");
            _currentProjector = _currentExpression;
        }

        public Expression CurrentProjector
        {
            get { return _currentProjector; }
            set { _currentProjector = value; }
        }

        public Expression CurrentExpression
        {
            get { return _currentExpression; }
            set { _currentExpression = value; }
        }
    }
}
