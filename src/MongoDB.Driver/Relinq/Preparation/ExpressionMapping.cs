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

using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Relinq.Preparation
{
    internal class ExpressionMapping
    {
        private readonly Dictionary<Expression, Expression> _mapping;

        public ExpressionMapping()
        {
            _mapping = new Dictionary<Expression, Expression>();
        }

        public void AddMapping(Expression original, Expression replacement)
        {
            Ensure.IsNotNull(original, "original");
            Ensure.IsNotNull(replacement, "replacement");

            _mapping[original] = replacement;
        }

        public bool TryGetMapping(Expression original, out Expression replacement)
        {
            return _mapping.TryGetValue(original, out replacement);
        }
    }
}
