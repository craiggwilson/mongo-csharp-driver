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
    /// The type of a custom aggregation expression.
    /// </summary>
    internal enum LinqToMongoExpressionType
    {
        /// <summary>
        /// An aggregation expression.
        /// </summary>
        Aggregation = 1000,
        /// <summary>
        /// A collection expression.
        /// </summary>
        Collection,
        /// <summary>
        /// A distinct expression.
        /// </summary>
        Distinct,
        /// <summary>
        /// A document expression.
        /// </summary>
        Document,
        /// <summary>
        /// A field expression.
        /// </summary>
        Field,
        /// <summary>
        /// A group expression.
        /// </summary>
        Group,
        /// <summary>
        /// A grouped aggregate expression.
        /// </summary>
        GroupedAggregate,
        /// <summary>
        /// A match expression.
        /// </summary>
        Match,
        /// <summary>
        /// A pipeline expression.
        /// </summary>
        Pipeline,
        /// <summary>
        /// A project expression.
        /// </summary>
        Project,
        /// <summary>
        /// A root aggregation expression.
        /// </summary>
        RootAggregation,
        /// <summary>
        /// A scalar expression.
        /// </summary>
        Scalar,
        /// <summary>
        /// A skip/limit expression.
        /// </summary>
        SkipLimit,
        /// <summary>
        /// A sort expression.
        /// </summary>
        Sort
    }
}