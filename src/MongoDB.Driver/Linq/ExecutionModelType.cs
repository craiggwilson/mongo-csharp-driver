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

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// The type of an Execution Model.
    /// </summary>
    public enum ExecutionModelType
    {
        /// <summary>
        /// Represents a query that can be executed with the query engine.
        /// </summary>
        Query,
        /// <summary>
        /// Represents a query that must be executed using a pipeline.
        /// </summary>
        Pipeline
    }
}