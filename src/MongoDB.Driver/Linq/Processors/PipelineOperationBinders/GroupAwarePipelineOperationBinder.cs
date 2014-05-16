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
using System.Reflection;
using System.Text;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.PipelineOperationBinders
{
    internal abstract class GroupAwarePipelineOperationBinder : PipelineOperationBinder
    {
        // private fields
        private readonly Dictionary<Expression, GroupExpression> _groupMap;

        // constructors
        protected GroupAwarePipelineOperationBinder(Dictionary<Expression, GroupExpression> groupMap)
        {
            _groupMap = groupMap;
        }

        // protected methods
        protected override Expression VisitMember(MemberExpression node)
        {
            // First and Last don't take selector expressions in their method 
            // calls, unlike Average, Min, Max, etc...  MongoDB requires a selector.
            // Therefore, we will take g.Last().Member and translate it to g.Select(x => x.Member).Last()
            // in order to make working with this expression more normal.
            var newNode = new FirstLastAggregateNormalizer().Normalize(node);
            if (newNode != node)
            {
                return Visit(newNode);
            }
            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (IsAggregateOperation(node))
            {
                return BindAggregateExpression(node);
            }

            return base.VisitMethodCall(node);
        }

        protected void RegisterGroup(Expression projector, GroupExpression group)
        {
            _groupMap.Add(projector, group);
        }

        // private methods
        private Expression BindAggregateExpression(MethodCallExpression node)
        {
            node = (MethodCallExpression)base.VisitMethodCall(node);
            var source = node.Arguments[0];
            var group = GetGroup(source);
            if (group == null)
            {
                throw LinqErrors.UnableToAssociateAggregateToGroup(node);
            }

            var projector = GetProjector(source);
            if (projector == null)
            {
                throw LinqErrors.UnableToDetermineAggregateExpression(node);
            }

            LambdaExpression argument = null;
            switch (node.Method.Name)
            {
                case "Count":
                case "LongCount":
                    if (node.Arguments.Count == 1)
                    {
                        argument = Expression.Lambda(
                            Expression.Constant(1),
                            Expression.Parameter(node.Method.ReturnType, "result"));
                        break;
                    }
                    throw LinqErrors.UnsupportedOperatorOverloadInGroup(node);
                case "First":
                case "Last":
                    // we have already normalized First/Last calls to only have 1 argument...
                    if (node.Arguments.Count == 1)
                    {
                        argument = GetArgument((MethodCallExpression)source);
                        break;
                    }
                    throw LinqErrors.UnsupportedOperatorOverloadInGroup(node);
                case "Average":
                case "Max":
                case "Min":
                case "Sum":
                    if (node.Arguments.Count == 2)
                    {
                        argument = (LambdaExpression)StripQuotes(node.Arguments[1]);
                    }
                    else
                    {
                        argument = GetArgument((MethodCallExpression)source);
                    }
                    break;
                default:
                    return base.VisitMethodCall(node);
            }

            var aggregationType = GetAggregationType(node.Method.Name);
            return BindAggregate(node.Method.ReturnType, group, projector, argument, aggregationType);
        }

        private Expression BindAggregate(Type returnType, GroupExpression group, Expression projector, LambdaExpression argument, AggregationType aggregationType)
        {
            Expression argExpression = null;
            if (argument != null)
            {
                RegisterProjector(projector);
                RegisterParameterReplacement(argument.Parameters[0], projector);
                argExpression = Visit(argument.Body);
            }
            else
            {
                argExpression = projector;
            }

            var aggregation = new AggregationExpression(returnType, aggregationType, argExpression);

            return new GroupedAggregateExpression(group.CorrelationId, aggregation);
        }

        private AggregationType GetAggregationType(string methodName)
        {
            switch(methodName)
            {
                case "Average":
                    return AggregationType.Average;
                case "Count":
                case "LongCount":
                case "Sum":
                    return AggregationType.Sum;
                case "First":
                    return AggregationType.First;
                case "Last":
                    return AggregationType.Last;
                case "Max":
                    return AggregationType.Max;
                case "Min":
                    return AggregationType.Min;
            }

            throw LinqErrors.UnsupportedAggregateOperator(methodName);
        }

        private LambdaExpression GetArgument(MethodCallExpression source)
        {
            // If we are here, it's because we want the Lambda Expression for the Select...
            return GetLambda(source.Arguments[1]);
        }

        private GroupExpression GetGroup(Expression source)
        {
            switch (source.NodeType)
            {
                case ExpressionType.New:
                    return _groupMap[source];
                case ExpressionType.Call:
                    var call = (MethodCallExpression)source;
                    if(call.Method.Name == "Select")
                    {
                        return GetGroup(call.Arguments[0]);
                    }
                    break;
            }

            return null;
        }

        private Expression GetProjector(Expression source)
        {
            switch(source.NodeType)
            {
                case ExpressionType.New:
                    return ((NewExpression)source).Arguments[1];
                case ExpressionType.Call:
                    var call = (MethodCallExpression)source;
                    if(call.Method.Name == "Select")
                    {
                        return GetProjector(call.Arguments[0]);
                    }
                    break;
            }

            return null;
        }

        private bool IsAggregateOperation(MethodCallExpression node)
        {
            return IsLinqMethod(node) && IsRootedAtAGroup(node);
        }

        private bool IsRootedAtAGroup(MethodCallExpression node)
        {
            var source = Visit(node.Arguments[0]);
            if (source.NodeType == ExpressionType.New)
            {
                if (node.Arguments[0].Type.IsGenericType &&
                    node.Arguments[0].Type.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                {
                    return true;
                }
                if (node.Arguments[0].Type.IsGenericType &&
                    node.Arguments[0].Type.GetGenericTypeDefinition() == typeof(Grouping<,>))
                {
                    return true;
                }
            }
            else if(source.NodeType == ExpressionType.Call)
            {
                var call = (MethodCallExpression)source;
                if(IsLinqMethod(call, "Select") && call.Arguments.Count == 2)
                {
                    return IsRootedAtAGroup(call);
                }
            }

            return false;
        }

        // We need to normalize how first and last calls are represented.
        // both group.Last().Member and group.Select(x => x.Member).Last() are
        // valid representations.  We are going to make all of the look like
        // the latter...
        private class FirstLastAggregateNormalizer : LinqToMongoExpressionVisitor
        {
            public Expression Normalize(Expression node)
            {
                return Visit(node);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var members = new Stack<MemberInfo>();
                Expression currentNode = node;
                while (currentNode.NodeType == ExpressionType.MemberAccess)
                {
                    var mex = (MemberExpression)currentNode;
                    members.Push(mex.Member);
                    currentNode = mex.Expression;
                }

                // we are going to rewrite g.Last().Member to g.Select(x => x.Member).Last()
                var call = currentNode as MethodCallExpression;
                if (call != null && IsAggregateMethod(call.Method.Name))
                {
                    var source = Visit(call.Arguments[0]);
                    var typeArguments = call.Method.GetGenericArguments();
                    var parameter = Expression.Parameter(typeArguments[0], "x");

                    Expression lambdaBody = parameter;
                    while(members.Count > 0)
                    {
                        var currentMember = members.Pop();
                        lambdaBody = Expression.MakeMemberAccess(lambdaBody, currentMember);
                    }

                    var select = Expression.Call(
                        typeof(Enumerable),
                        "Select",
                        new[] { typeArguments[0], lambdaBody.Type },
                        source,
                        Expression.Lambda(
                            typeof(Func<,>).MakeGenericType(typeArguments[0], lambdaBody.Type),
                            lambdaBody,
                            parameter));

                    return Expression.Call(
                        typeof(Enumerable),
                        call.Method.Name,
                        new[] { lambdaBody.Type },
                        select);
                }

                return base.VisitMember(node);
            }

            private bool IsAggregateMethod(string methodName)
            {
                switch (methodName)
                {
                    case "First":
                    case "Last":
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}