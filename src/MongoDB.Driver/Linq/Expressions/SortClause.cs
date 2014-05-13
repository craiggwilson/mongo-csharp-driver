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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// An expression and it's direction to sort by.
    /// </summary>
    internal class SortClause
    {
        // private fields
        private readonly SortDirection _direction;
        private readonly Expression _expression;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SortClause" /> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="direction">The direction.</param>
        public SortClause(Expression expression, SortDirection direction)
        {
            _expression = expression;
            _direction = direction;
        }

        // public properties
        /// <summary>
        /// Gets the direction.
        /// </summary>
        public SortDirection Direction
        {
            get { return _direction; }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
        }
    }
}