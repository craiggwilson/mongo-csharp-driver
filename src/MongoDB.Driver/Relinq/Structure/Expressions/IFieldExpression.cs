/* Copyright 2015 MongoDB Inc.
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
namespace MongoDB.Driver.Relinq.Structure.Expressions
{
    internal interface IFieldExpression
    {
        string FieldName { get; }

        IBsonSerializer Serializer { get; }
    }

    internal static class IFieldExpressionExtensions
    {
        public static string AppendFieldName(this IFieldExpression expression, string suffix)
        {
            return CombineFieldNames(expression == null ? null : expression.FieldName, suffix);
        }

        public static string PrependFieldName(this IFieldExpression expression, string prefix)
        {
            return CombineFieldNames(prefix, expression == null ? null : expression.FieldName);
        }

        private static string CombineFieldNames(string prefix, string suffix)
        {
            if (prefix == null)
            {
                return suffix;
            }
            if (suffix == null)
            {
                return prefix;
            }

            return prefix + "." + suffix;
        }
    }
}
