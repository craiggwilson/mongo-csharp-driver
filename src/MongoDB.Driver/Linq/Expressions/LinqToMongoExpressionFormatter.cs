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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MongoDB.Driver.Linq.Expressions
{
    internal class LinqToMongoExpressionFormatter : LinqToMongoExpressionVisitor
    {
        // private fields
        private StringBuilder _sb;

        // constructors
        public LinqToMongoExpressionFormatter()
        {
            _sb = new StringBuilder();
        }

        // public methods
        public override string ToString()
        {
            return _sb.ToString();
        }

        // protected methods
        protected override Expression VisitAggregation(AggregationExpression node)
        {
            _sb.AppendFormat("<{0}:", node.AggregationType);
            Visit(node.Argument);
            _sb.AppendFormat(">");
            return node;
        }

        // protected methods
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                Visit(node.Left);
                _sb.Append("[");
                Visit(node.Right);
                _sb.Append("]");
                return node;
            }

            _sb.Append("(");
            Visit(node.Left);
            switch (node.NodeType)
            {
                case ExpressionType.Add: _sb.Append(" + "); break;
                case ExpressionType.And: _sb.Append(" & "); break;
                case ExpressionType.AndAlso: _sb.Append(" && "); break;
                case ExpressionType.Coalesce: _sb.Append(" ?? "); break;
                case ExpressionType.Divide: _sb.Append(" / "); break;
                case ExpressionType.Equal: _sb.Append(" == "); break;
                case ExpressionType.ExclusiveOr: _sb.Append(" ^ "); break;
                case ExpressionType.GreaterThan: _sb.Append(" > "); break;
                case ExpressionType.GreaterThanOrEqual: _sb.Append(" >= "); break;
                case ExpressionType.LeftShift: _sb.Append(" << "); break;
                case ExpressionType.LessThan: _sb.Append(" < "); break;
                case ExpressionType.LessThanOrEqual: _sb.Append(" <= "); break;
                case ExpressionType.Modulo: _sb.Append(" % "); break;
                case ExpressionType.Multiply: _sb.Append(" * "); break;
                case ExpressionType.NotEqual: _sb.Append(" != "); break;
                case ExpressionType.Or: _sb.Append(" | "); break;
                case ExpressionType.OrElse: _sb.Append(" || "); break;
                case ExpressionType.RightShift: _sb.Append(" >> "); break;
                case ExpressionType.Subtract: _sb.Append(" - "); break;
                case ExpressionType.TypeAs: _sb.Append(" as "); break;
                case ExpressionType.TypeIs: _sb.Append(" is "); break;
                default: _sb.AppendFormat(" <{0}> ", node.NodeType); break;
            }
            Visit(node.Right);
            _sb.Append(")");
            return node;
        }

        protected override Expression VisitCollection(CollectionExpression node)
        {
            _sb.AppendFormat("<Collection:{0}>", node.DocumentType.Name);
            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            _sb.Append("<ConditionalExpression>");
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // need to check node.Type instead of value.GetType() because boxed Nullable<T> values are boxed as <T>
            if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                _sb.AppendFormat("({0})", FriendlyTypeName(node.Type));
            }
            VisitValue(node.Value);
            return node;
        }

        protected override Expression VisitDistinct(DistinctExpression node)
        {
            Visit(node.Source);
            _sb.Append(".<Distinct(");
            Visit(node.Projector);
            _sb.Append(")>");
            return node;
        }

        protected override Expression VisitDocument(DocumentExpression node)
        {
            Visit(node.Expression);
            return node;
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            _sb.Append("<ElementInit>");
            return node;
        }

        protected override IEnumerable<ElementInit> VisitElementInitList(ReadOnlyCollection<ElementInit> nodes)
        {
            _sb.Append("<ReadOnlyCollection<ElementInit>>");
            return nodes;
        }

        protected override Expression VisitField(FieldExpression node)
        {
            if (node.SerializationInfo != null)
            {
                _sb.AppendFormat("<Field:{0}>", node.SerializationInfo.ElementName ?? "(null)");
            }
            else
            {
                _sb.Append("<Field>");
            }

            return node;
        }

        protected override Expression VisitGroup(GroupExpression node)
        {
            Visit(node.Source);
            _sb.Append(".<Group(");
            Visit(node.Id);
            if (node.Aggregations != null && node.Aggregations.Count > 0)
            {
                foreach(var agg in node.Aggregations)
                {
                    _sb.Append(", ");
                    Visit(agg);
                }
            }
            _sb.Append(")");
            return node;
        }

        protected override Expression VisitGroupedAggregate(GroupedAggregateExpression node)
        {
            _sb.Append("<GroupedAggregate(");
            Visit(node.Aggregation);
            _sb.Append(")>");
            return node;
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            _sb.Append("<InvocationExpression>");
            return node;
        }

        protected override Expression VisitLambda(LambdaExpression node)
        {
            _sb.Append("(");
            _sb.Append(string.Join(", ", node.Parameters.Select(p => FriendlyTypeName(p.Type) + " " + p.Name).ToArray()));
            _sb.Append(") => ");
            Visit(node.Body);
            return node;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            _sb.Append("<ListInitExpression>");
            return node;
        }

        protected override Expression VisitMatch(MatchExpression node)
        {
            Visit(node.Source);
            _sb.Append(".<Match(");
            Visit(node.Match);
            _sb.Append(")>");
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Visit(node.Expression);
            _sb.Append(".");
            _sb.Append(node.Member.Name);
            return node;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            _sb.Append("<MemberAssignment>");
            return node;
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            _sb.Append("<MemberBinding>");
            return node;
        }

        protected override IEnumerable<MemberBinding> VisitMemberBindingList(ReadOnlyCollection<MemberBinding> nodes)
        {
            _sb.Append("<ReadOnlyCollection<MemberBinding>>");
            return nodes;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            _sb.Append("<MemberInitExpression>");
            return node;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            _sb.Append("<MemberListBinding>");
            return node;
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            _sb.Append("<MemberMemberBinding>");
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsStatic)
            {
                _sb.Append(FriendlyTypeName(node.Method.DeclaringType));
            }
            else
            {
                Visit(node.Object);
            }
            _sb.Append(".");
            _sb.Append(node.Method.Name);
            if (node.Method.IsGenericMethod)
            {
                _sb.AppendFormat("<{0}>", string.Join(", ", node.Method.GetGenericArguments().Select(t => FriendlyTypeName(t)).ToArray()));
            }
            _sb.Append("(");
            var separator = "";
            foreach (var arg in node.Arguments)
            {
                _sb.Append(separator);
                Visit(arg);
                separator = ", ";
            }
            _sb.Append(")");
            return node;
        }

        protected override NewExpression VisitNew(NewExpression node)
        {
            _sb.Append("new ");
            _sb.Append(FriendlyTypeName(node.Type));
            _sb.Append("(");
            var separator = "";
            foreach (var arg in node.Arguments)
            {
                _sb.Append(separator);
                Visit(arg);
                separator = ", ";
            }
            _sb.Append(")");
            return node;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var elementType = node.Type.GetElementType();
            _sb.AppendFormat("new {0}[] {{ ", FriendlyTypeName(elementType));
            var separator = "";
            foreach (var item in node.Expressions)
            {
                _sb.Append(separator);
                Visit(item);
                separator = ", ";
            }
            _sb.Append(" }");
            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _sb.Append(node.Name);
            return node;
        }

        protected override Expression VisitPipeline(PipelineExpression node)
        {
            Visit(node.Source);
            _sb.Append(".<Pipeline(");
            Visit(node.Projector);
            _sb.Append(")>");
            return node;
        }

        protected override Expression VisitProject(ProjectExpression node)
        {
            Visit(node.Source);
            _sb.Append(".<Project(");
            Visit(node.Projector);
            _sb.Append(")>");
            return node;
        }

        protected override Expression VisitRootAggregation(RootAggregationExpression node)
        {
            Visit(node.Source);
            _sb.AppendFormat(".<{0}(", node.AggregationType);
            switch(node.AggregationType)
            {
                case RootAggregationType.Count:
                    break;
                default:
                    Visit(node.Projector);
                    break;
            }
            _sb.Append(")>");
            return node;
        }

        protected override Expression VisitSkipLimit(SkipLimitExpression node)
        {
            Visit(node.Source);
            _sb.Append(".<SkipLimit(");
            Visit(node.Skip);
            _sb.Append(",");
            Visit(node.Limit);
            _sb.Append(")>");
            return node;
        }

        protected override Expression VisitSort(SortExpression node)
        {
            Visit(node.Source);
            var fields = string.Join(", ",
                node.SortClauses.Select(x =>
                    string.Format("{0} {1}",
                    x.Expression is FieldExpression ? ((FieldExpression)x.Expression).SerializationInfo.ElementName : x.ToString(),
                    x.Direction)).ToArray());

            _sb.AppendFormat(".<Sort({0})>", fields);
            return node;
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            _sb.Append("(");
            Visit(node.Expression);
            _sb.Append(" is ");
            _sb.Append(FriendlyTypeName(node.TypeOperand));
            _sb.Append(")");
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.ArrayLength: break;
                case ExpressionType.Convert: _sb.AppendFormat("({0})", FriendlyTypeName(node.Type)); break;
                case ExpressionType.Negate: _sb.Append("-"); break;
                case ExpressionType.Not: _sb.Append("!"); break;
                case ExpressionType.Quote: break;
                case ExpressionType.UnaryPlus: _sb.Append("+"); break;
                default: _sb.AppendFormat("<{0}>", node.NodeType); break;
            }
            Visit(node.Operand);
            switch (node.NodeType)
            {
                case ExpressionType.ArrayLength: _sb.Append(".Length"); break;
            }
            return node;
        }

        // private methods
        private string FriendlyTypeName(Type type)
        {
            var typeName = IsAnonymousType(type) ? "__AnonymousType" : type.Name;

            if (type.IsGenericType)
            {
                var sb = new StringBuilder();
                sb.AppendFormat("{0}<", Regex.Replace(typeName, @"\`\d+$", ""));
                foreach (var typeParameter in type.GetGenericArguments())
                {
                    sb.AppendFormat("{0}, ", FriendlyTypeName(typeParameter));
                }
                sb.Remove(sb.Length - 2, 2);
                sb.Append(">");
                return sb.ToString();
            }
            else
            {
                return typeName;
            }
        }

        private bool IsAnonymousType(Type type)
        {
            // don't test for too many things in case implementation details change in the future
            return
                Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false) &&
                type.IsGenericType &&
                type.Name.Contains("Anon"); // don't check for more than "Anon" so it works in mono also
        }

        private void VisitValue(object value)
        {
            if (value == null)
            {
                _sb.Append("null");
                return;
            }

            var a = value as Array;
            if (a != null && a.Rank == 1)
            {
                var elementType = a.GetType().GetElementType();
                _sb.AppendFormat("{0}[]:{{", FriendlyTypeName(elementType));
                var separator = " ";
                foreach (var item in a)
                {
                    _sb.Append(separator);
                    VisitValue(item);
                    separator = ", ";
                }
                _sb.Append(" }");
                return;
            }

            if (value.GetType() == typeof(bool))
            {
                _sb.Append(((bool)value) ? "true" : "false");
                return;
            }

            if (value.GetType() == typeof(char))
            {
                var c = (char)value;
                _sb.AppendFormat("'{0}'", c.ToString());
                return;
            }

            if (value.GetType() == typeof(DateTime))
            {
                var dt = (DateTime)value;

                var formatted = dt.ToString("o");
                formatted = Regex.Replace(formatted, @"\.0000000", "");
                formatted = Regex.Replace(formatted, @"0000[Z+-]", "");

                if (dt.Kind == DateTimeKind.Utc)
                {
                    _sb.AppendFormat("DateTime:({0})", formatted);
                }
                else
                {
                    _sb.AppendFormat("DateTime:({0}, {1})", formatted, dt.Kind);
                }
                return;
            }

            var e = value as Enum;
            if (e != null)
            {
                _sb.Append(FriendlyTypeName(e.GetType()) + "." + e.ToString());
                return;
            }

            var regex = value as Regex;
            if (regex != null)
            {
                var pattern = regex.ToString();
                var options = regex.Options;
                _sb.Append("Regex:(@\"");
                _sb.Append(pattern);
                _sb.Append("\"");
                if (options != RegexOptions.None)
                {
                    _sb.AppendFormat(", {0}", options.ToString());
                }
                _sb.Append(")");
                return;
            }

            var s = value as string;
            if (s != null)
            {
                s = Regex.Replace(s, @"([""\\])", @"\\$1");
                _sb.Append("\"");
                _sb.Append(s);
                _sb.Append("\"");
                return;
            }

            if (value.GetType() == typeof(TimeSpan))
            {
                var ts = (TimeSpan)value;
                _sb.AppendFormat("TimeSpan:({0})", ts.ToString());
                return;
            }

            var type = value as Type;
            if (type != null)
            {
                _sb.AppendFormat("typeof({0})", FriendlyTypeName(type));
                return;
            }

            _sb.Append(value.ToString());
        }

        // public static methods
        public static string ToString(Expression node)
        {
            var formatter = new LinqToMongoExpressionFormatter();
            formatter.Visit(node);
            return formatter.ToString();
        }
    }
}
