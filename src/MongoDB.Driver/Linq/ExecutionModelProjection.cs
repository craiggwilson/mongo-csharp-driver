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
using System.Linq.Expressions;
using System.Text;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Represents a projection from an execution model.
    /// </summary>
    internal class ExecutionModelProjection
    {
        // private fields
        private IEnumerable<BsonSerializationInfo> _fieldSerializationInfo;
        private LambdaExpression _projector;

        // public properties
        /// <summary>
        /// Gets or sets the field serialization info.
        /// </summary>
        public IEnumerable<BsonSerializationInfo> FieldSerializationInfo
        {
            get { return _fieldSerializationInfo; }
            set { _fieldSerializationInfo = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the projection has fields.
        /// </summary>
        public bool HasFields
        {
            get { return _fieldSerializationInfo != null && _fieldSerializationInfo.Any(); }
        }

        /// <summary>
        /// Gets or sets the projector.
        /// </summary>
        public LambdaExpression Projector
        {
            get { return _projector; }
            set { _projector = value; }
        }
    }
}