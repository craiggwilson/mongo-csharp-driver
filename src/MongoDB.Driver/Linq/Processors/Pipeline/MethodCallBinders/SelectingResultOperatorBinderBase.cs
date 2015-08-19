using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.Pipeline.MethodCallBinders
{
    internal abstract class SelectingResultOperatorBinderBase : IMethodCallBinder<PipelineBindingContext>
    {
        public Expression Bind(PipelineExpression pipeline, PipelineBindingContext bindingContext, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            var source = pipeline.Source;
            Expression argument;
            if (arguments.Any())
            {
                var lambda = ExpressionHelper.GetLambda(arguments.Single());
                bindingContext.AddExpressionMapping(lambda.Parameters[0], pipeline.Projector);

                argument = bindingContext.Bind(lambda.Body);
            }
            else
            {
                var selectExpression = source as SelectExpression;
                if (selectExpression != null)
                {
                    source = selectExpression.Source;
                    argument = selectExpression.Selector;
                    var fieldAsDocumentExpression = argument as FieldAsDocumentExpression;
                    if (fieldAsDocumentExpression != null)
                    {
                        argument = fieldAsDocumentExpression.Expression;
                    }
                }
                else
                {
                    argument = pipeline.Projector;
                }
            }

            var serializer = bindingContext.GetSerializer(node.Type, argument);

            var accumulator = new AccumulatorExpression(
                node.Type,
                "__result",
                serializer,
                GetAccumulatorType(),
                argument);

            source = new GroupByExpression(
                node.Type,
                source,
                Expression.Constant(1),
                new[] { accumulator });

            return new PipelineExpression(
                source,
                new FieldExpression(accumulator.FieldName, accumulator.Serializer),
                CreateResultOperator(node.Type));
        }

        protected abstract AccumulatorType GetAccumulatorType();

        protected abstract ResultOperator CreateResultOperator(Type resultType);
    }
}
