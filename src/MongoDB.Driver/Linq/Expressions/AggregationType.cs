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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// The type of an AggregateExpression.
    /// </summary>
    /// <remarks>
    /// These are the aggregation expressions supported by MongoDB.  Count, for instance,
    /// is not one of these as it is handled in MongoDB by a { $sum : 1 } and is therefore
    /// represented here as a Sum aggregation expression.
    /// </remarks>
    internal enum AggregationType
    {
        /// <summary>
        /// An addToSet aggregate.
        /// </summary>
        AddToSet,
        /// <summary>
        /// An avg aggregate.
        /// </summary>
        Average,
        /// <summary>
        /// A first aggregate.
        /// </summary>
        First,
        /// <summary>
        /// A last aggregate.
        /// </summary>
        Last,
        /// <summary>
        /// A min aggregate.
        /// </summary>
        Min,
        /// <summary>
        /// A max aggregate.
        /// </summary>
        Max,
        /// <summary>
        /// A push aggregate.
        /// </summary>
        Push,
        /// <summary>
        /// A sum aggregate.
        /// </summary>
        Sum        
    }
}