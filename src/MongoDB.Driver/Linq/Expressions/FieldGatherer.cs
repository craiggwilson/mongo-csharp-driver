using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <summary>
    /// Gathers FieldExpressions.
    /// </summary>
    internal class FieldGatherer : LinqToMongoExpressionVisitor
    {
        private List<FieldExpression> _fields;
        private bool _includeProjectedFields;

        public ReadOnlyCollection<FieldExpression> Gather(Expression node, bool includeProjectedFields)
        {
            _fields = new List<FieldExpression>();
            _includeProjectedFields = includeProjectedFields;
            Visit(node);
            return _fields.AsReadOnly();
        }

        protected override Expression VisitField(FieldExpression node)
        {
            if (!_includeProjectedFields && node.IsProjected)
            {
                Visit(node.Expression);
            }
            else
            {
                _fields.Add(node);
            }
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!IsLinqMethod(node, "Select"))
            {
                return base.VisitMethodCall(node);
            }

            var source = node.Arguments[0] as FieldExpression;
            if (source != null)
            {
                var fields = new FieldGatherer().Gather(node.Arguments[1], _includeProjectedFields);
                if (fields.Any(x => x.SerializationInfo.ElementName.StartsWith(source.SerializationInfo.ElementName)))
                {
                    _fields.AddRange(fields);
                    return node;
                }
            }

            return base.VisitMethodCall(node);
        }
    }
}
