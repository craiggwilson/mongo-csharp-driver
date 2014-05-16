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
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq
{
    internal static class LinqErrors
    {
        internal static Exception UnexpectedFieldInProjection(string currentKey)
        {
            return new MongoLinqException("Some fields have come back from the server that were not expected.");
        }

        internal static Exception UnreachableCode(string description)
        {
            var message = string.Format("Unreachable code detected: {0}", description);
            return new MongoLinqException(message);
        }

        internal static Exception UnsupportedExpression(Expression node)
        {
            return new MongoLinqException(string.Format("Unhandled expression type: '{0}'", node.NodeType));
        }

        internal static Exception UnableToAssociateAggregateToGroup(MethodCallExpression node)
        {
            var message = string.Format("The aggregate '{0}' could not be associated to a group.", LinqToMongoExpressionFormatter.ToString(node));
            return new MongoLinqException(message);
        }

        internal static Exception UnableToDetermineAggregateExpression(MethodCallExpression node)
        {
            var message = string.Format("Could not determine the aggregate expression from '{0}'.", LinqToMongoExpressionFormatter.ToString(node));
            return new MongoLinqException(message);
        }

        internal static Exception UnsupportedOperatorOverloadInGroup(MethodCallExpression node)
        {
            var message = string.Format("Unsupported overload of the operator '{0}' in a group.", node.Method.Name);
            return new MongoLinqException(message);
        }

        internal static Exception UnsupportedAggregateOperator(string methodName)
        {
            var message = string.Format("{0} is not a supported aggregate operator.", methodName);
            return new MongoLinqException(message);
        }

        internal static Exception UnsupportedAggregateOperator(Expression node)
        {
            var message = string.Format("{0} is not a supported aggregate operator.", LinqToMongoExpressionFormatter.ToString(node));
            return new MongoLinqException(message);
        }

        internal static Exception UnsupportedGroupingKey(Expression key)
        {
            var message = string.Format("The expression '{0}' is not a valid grouping key.  It must be a field or an anonymous type.", LinqToMongoExpressionFormatter.ToString(key));
            return new MongoLinqException(message);
        }

        internal static Exception UnsupportedQueryOperator(MemberExpression node)
        {
            var message = string.Format("The {0} query operator is not supported.", node.Member.Name);
            return new MongoLinqException(message);
        }

        internal static Exception UnsupportedQueryOperator(MethodCallExpression node)
        {
            var message = string.Format("The {0} query operator is not supported.", node.Method.Name);
            return new MongoLinqException(message);
        }

        internal static Exception UnsupportedQueryOperatorOverload(MethodCallExpression node)
        {
            var message = string.Format("The overload of the query operator '{0}' is not supported.", node.Method.Name);
            return new MongoLinqException(message);
        }

        internal static Exception InvalidSource(Expression source)
        {
            return new MongoLinqException(string.Format("The expression of type '{0}' is not a valid source.", source.Type));
        }

        internal static Exception UnmappedMember(MemberExpression node)
        {
            return new MongoLinqException("Mapped member was found to not carry serialization information.");
        }

        internal static Exception InvalidProjection(Expression projection)
        {
            return new MongoLinqException(string.Format("The expression '{0}' is not a valid projection.", LinqToMongoExpressionFormatter.ToString(projection)));
        }

        internal static Exception UnsupportedPipelineOperator(Expression node)
        {
            var message = string.Format("{0} is not supported in a $project or $group node.", node);
            return new MongoLinqException(message);
        }

        internal static Exception Unsupported(Expression node, ExecutionTarget executionTarget, Exception inner)
        {
            var message = string.Format("The query contains portions that are too complex or not supported using the chosen query target of {0}.", executionTarget);
            return new MongoLinqException(message, inner);
        }

        internal static Exception Unsupported()
        {
            return new MongoLinqException("The query is too complex.");
        }
    }
}