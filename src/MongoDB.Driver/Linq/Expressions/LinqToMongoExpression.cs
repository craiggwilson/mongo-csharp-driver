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
using System.Linq.Expressions;
using System.Text;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// Base expression class for custom Aggregation expressions.
    /// </summary>
    internal abstract class LinqToMongoExpression : Expression
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqToMongoExpression" /> class.
        /// </summary>
        /// <param name="expressionType">Type of the expression.</param>
        /// <param name="type">The type.</param>
        protected LinqToMongoExpression(LinqToMongoExpressionType expressionType, Type type)
#pragma warning disable 618
            : base((ExpressionType)expressionType, type)
#pragma warning restore  618
        {
        }
    }
}