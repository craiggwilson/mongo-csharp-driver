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

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// The target of a linq query.
    /// </summary>
    [Flags]
    public enum ExecutionTarget
    {
        /// <summary>
        /// Linq queries are not allowed.
        /// </summary>
        None = 0,
        /// <summary>
        /// Linq queries can target the query engine.
        /// </summary>
        Query = 1,
        /// <summary>
        /// Linq queries can target the pipeline engine.
        /// </summary>
        Pipeline = 2,
        /// <summary>
        /// Linq queries should target the best model.
        /// </summary>
        Best = 7, // in case we add an M/R target...
    }
}