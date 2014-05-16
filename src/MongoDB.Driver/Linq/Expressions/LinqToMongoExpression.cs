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
using System.Linq.Expressions;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// Base expression class for custom LinqToMongo expressions.
    /// </summary>
    internal abstract class LinqToMongoExpression : Expression
    {
        // constructors
        protected LinqToMongoExpression(LinqToMongoExpressionType expressionType, Type type)
#pragma warning disable 618
            // TODO: need to build this correctly to .NET 4 standards, although this
            // works perfectly fine.
            : base((ExpressionType)expressionType, type)
#pragma warning restore  618
        {
        }
    }
}