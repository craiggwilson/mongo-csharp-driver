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

using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// Marks an Expression that contains serialization information.
    /// </summary>
    internal interface IBsonSerializationInfoExpression
    {
        /// <summary>
        /// Gets a value indicating whether this serialization info is the result of a projection.
        /// </summary>
        bool IsProjected { get; }

        /// <summary>
        /// Gets the serialization info.
        /// </summary>
        BsonSerializationInfo SerializationInfo { get; }
    }
}