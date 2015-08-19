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
using MongoDB.Driver.Linq.Processors.Pipeline.MethodCallBinders;

namespace MongoDB.Driver.Linq.Processors.Pipeline
{
    internal sealed class PipelineBinder : PipelineBinderBase<PipelineBindingContext>
    {
        private readonly static CompositeMethodCallBinder<PipelineBindingContext> __methodCallBinder;

        static PipelineBinder()
        {
            var infoBinder = new MethodInfoMethodCallBinder<PipelineBindingContext>();
            infoBinder.Register(AnyBinder.GetSupportedMethods(), new AnyBinder());
            infoBinder.Register(AverageBinder.GetSupportedMethods(), new AverageBinder());
            infoBinder.Register(CountBinder.GetSupportedMethods(), new CountBinder());
            infoBinder.Register(DistinctBinder.GetSupportedMethods(), new DistinctBinder());
            infoBinder.Register(FirstBinder.GetSupportedMethods(), new FirstBinder());
            infoBinder.Register(GroupByBinder.GetSupportedMethods(), new GroupByBinder());
            infoBinder.Register(GroupByWithResultSelectorBinder.GetSupportedMethods(), new GroupByWithResultSelectorBinder());
            infoBinder.Register(MaxBinder.GetSupportedMethods(), new MaxBinder());
            infoBinder.Register(MinBinder.GetSupportedMethods(), new MinBinder());
            infoBinder.Register(OfTypeBinder.GetSupportedMethods(), new OfTypeBinder());
            infoBinder.Register(OrderByBinder.GetSupportedMethods(), new OrderByBinder());
            infoBinder.Register(SelectBinder.GetSupportedMethods(), new SelectBinder());
            infoBinder.Register(SelectManyBinder.GetSupportedMethods(), new SelectManyBinder());
            infoBinder.Register(SingleBinder.GetSupportedMethods(), new SingleBinder());
            infoBinder.Register(SkipBinder.GetSupportedMethods(), new SkipBinder());
            infoBinder.Register(SumBinder.GetSupportedMethods(), new SumBinder());
            infoBinder.Register(TakeBinder.GetSupportedMethods(), new TakeBinder());
            infoBinder.Register(ThenByBinder.GetSupportedMethods(), new ThenByBinder());
            infoBinder.Register(WhereBinder.GetSupportedMethods(), new WhereBinder());

            var nameBinder = new NameBasedMethodCallBinder<PipelineBindingContext>();
            __methodCallBinder = new CompositeMethodCallBinder<PipelineBindingContext>(
                infoBinder,
                nameBinder);
        }

        public static Expression Bind(Expression node, IBsonSerializer rootSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var bindingContext = new PipelineBindingContext(serializerRegistry);
            var binder = new PipelineBinder(bindingContext, rootSerializer);

            node = binder.Bind(node);
            node = AccumulatorBinder.Bind(node, bindingContext);
            node = CorrelatedGroupRewriter.Rewrite(node);
            return node;
        }

        private readonly IBsonSerializer _rootSerializer;

        private PipelineBinder(PipelineBindingContext bindingContext, IBsonSerializer rootSerializer)
            : base(bindingContext, __methodCallBinder)
        {
            _rootSerializer = rootSerializer;
        }

        protected override Expression BindNonMethodCall(Expression node)
        {
            if (node.NodeType == ExpressionType.Constant &&
                node.Type.IsGenericType &&
                node.Type.GetGenericTypeDefinition() == typeof(IMongoQueryable<>))
            {
                var inputType = node.Type.GetGenericArguments()[0];

                return new PipelineExpression(
                    new CollectionExpression(_rootSerializer),
                    new DocumentExpression(_rootSerializer));
            }

            var message = string.Format("The expression tree is not supported: {0}",
                            node.ToString());

            throw new NotSupportedException(message);
        }
    }
}