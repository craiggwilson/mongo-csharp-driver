﻿/* Copyright 2010-2012 10gen Inc.
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

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// The type of an <see cref="AggregationExpression"/>.
    /// </summary>
    /// <remarks>
    /// These are the aggregation expressions supported by MongoDB. Count, for instance,
    /// is not one of these as it is handled in MongoDB by a { $sum : 1 } and would therefore
    /// be represented as a Sum aggregation expression.
    /// </remarks>
    internal enum AggregationType
    {
        Average,
        First,
        Last,
        Min,
        Max,
        Push,
        Sum        
    }
}