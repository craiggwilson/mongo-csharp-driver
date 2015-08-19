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

using System;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors.EmbeddedPipeline.MethodCallBinders;
using MongoDB.Driver.Support;

namespace MongoDB.Driver.Linq.Processors.EmbeddedPipeline
{
    internal sealed class EmbeddedPipelineBinder : PipelineBinderBase<EmbeddedPipelineBindingContext>
    {
        private readonly static CompositeMethodCallBinder<EmbeddedPipelineBindingContext> __methodCallBinder;

        static EmbeddedPipelineBinder()
        {
            var infoBinder = new MethodInfoMethodCallBinder<EmbeddedPipelineBindingContext>();
            infoBinder.Register(AllBinder.GetSupportedMethods(), new AllBinder());
            infoBinder.Register(AnyBinder.GetSupportedMethods(), new AnyBinder());
            infoBinder.Register(AverageBinder.GetSupportedMethods(), new AverageBinder());
            infoBinder.Register(DistinctBinder.GetSupportedMethods(), new DistinctBinder());
            infoBinder.Register(ExceptBinder.GetSupportedMethods(), new ExceptBinder());
            infoBinder.Register(FirstBinder.GetSupportedMethods(), new FirstBinder());
            infoBinder.Register(IntersectBinder.GetSupportedMethods(), new IntersectBinder());
            infoBinder.Register(LastBinder.GetSupportedMethods(), new LastBinder());
            infoBinder.Register(MaxBinder.GetSupportedMethods(), new MaxBinder());
            infoBinder.Register(MinBinder.GetSupportedMethods(), new MinBinder());
            infoBinder.Register(SelectBinder.GetSupportedMethods(), new SelectBinder());
            infoBinder.Register(SumBinder.GetSupportedMethods(), new SumBinder());
            infoBinder.Register(ToArrayBinder.GetSupportedMethods(), new ToArrayBinder());
            infoBinder.Register(ToHashSetBinder.GetSupportedMethods(), new ToHashSetBinder());
            infoBinder.Register(ToListBinder.GetSupportedMethods(), new ToListBinder());
            infoBinder.Register(UnionBinder.GetSupportedMethods(), new UnionBinder());
            infoBinder.Register(WhereBinder.GetSupportedMethods(), new WhereBinder());

            var nameBinder = new NameBasedMethodCallBinder<EmbeddedPipelineBindingContext>();
            nameBinder.Register(new ContainsBinder(), ContainsBinder.IsSupported, ContainsBinder.SupportedMethodNames);
            nameBinder.Register(new CountBinder(), CountBinder.IsSupported, CountBinder.SupportedMethodNames);

            __methodCallBinder = new CompositeMethodCallBinder<EmbeddedPipelineBindingContext>(
                infoBinder,
                nameBinder);
        }

        public static bool SupportsNode(MethodCallExpression node)
        {
            return __methodCallBinder.IsRegistered(node);
        }

        public static Expression Bind(Expression node, IBindingContext parent)
        {
            var bindingContext = new EmbeddedPipelineBindingContext(parent);
            var binder = new EmbeddedPipelineBinder(bindingContext);

            var bound = binder.Bind(node);
            bound = AccumulatorBinder.Bind(bound, bindingContext);
            return CorrelatedGroupRewriter.Rewrite(bound);
        }

        public EmbeddedPipelineBinder(EmbeddedPipelineBindingContext bindingContext)
            : base(bindingContext, __methodCallBinder)
        {
        }

        protected override Expression BindNonMethodCall(Expression node)
        {
            var serializationExpression = node as ISerializationExpression;
            if (serializationExpression != null)
            {
                var arraySerializer = serializationExpression.Serializer as IBsonArraySerializer;
                BsonSerializationInfo itemSerializationInfo;
                if (arraySerializer != null && arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                {
                    return new PipelineExpression(
                        node,
                        new DocumentExpression(itemSerializationInfo.Serializer));
                }
            }
            else if (node.NodeType == ExpressionType.Constant)
            {
                var sequenceType = node.Type.GetSequenceElementType();
                if (sequenceType != null)
                {
                    return new PipelineExpression(
                        node,
                        Expression.Parameter(sequenceType, "document"));
                }
            }

            var message = string.Format("The expression tree is not supported: {0}",
                            node.ToString());

            throw new NotSupportedException(message);
        }
    }
}