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
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors
{
    /// <remarks>
    /// A GroupedAggregateExpression is a temporary expression whose mere presence 
    /// indicates a need to move it around.  So, we find all of these that exist first
    /// and then come back and add them into the GroupExpression and remove them from
    /// the tree.
    /// </remarks>
    internal class GroupedAggregateRewriter : LinqToMongoExpressionVisitor
    {
        private Dictionary<GroupedAggregateExpression, Expression> _map;
        private ILookup<Guid, GroupedAggregateExpression> _lookup;

        // public methods
        public Expression Rewrite(Expression node)
        {
            _map = new Dictionary<GroupedAggregateExpression, Expression>();
            return Visit(node);
        }

        // protected methods
        protected override Expression VisitField(FieldExpression node)
        {
            var exp = Visit(node.Expression);
            if (exp != node.Expression)
            {
                return new FieldExpression(exp, node.SerializationInfo, node.IsProjected);
            }

            return base.VisitField(node);
        }

        protected override Expression VisitGroupedAggregate(GroupedAggregateExpression node)
        {
            Expression mapped;
            if (_map.TryGetValue(node, out mapped))
            {
                return mapped;
            }

            return base.VisitGroupedAggregate(node);
        }

        protected override Expression VisitGroup(GroupExpression node)
        {
            if (_lookup != null && _lookup.Contains(node.CorrelationId))
            {
                var source = Visit(node.Source);
                var aggregations = new List<FieldExpression>();

                // Sometimes, an aggregation could appear in multiple places in a tree.
                // For instance, 
                //    group by a.B into g 
                //    orderby g.Count()
                //    select g.Count();
                // We don't want to have the g.Count() aggregate in our GroupExpression
                // twice, so we make sure it's only there once and map the second
                // one to the first one.
                foreach (var aggregation in _lookup[node.CorrelationId])
                {
                    var index = aggregations.FindIndex(x => new ExpressionComparer().Compare(x.Expression, aggregation.Aggregation));
                    
                    if (index == -1)
                    {
                        var serializer = BsonSerializer.LookupSerializer(aggregation.Type);
                        var info = new BsonSerializationInfo(
                            "_agg" + aggregations.Count,
                            serializer,
                            aggregation.Type);

                        var field = new FieldExpression(aggregation.Aggregation, info, true);
                        aggregations.Add(field);
                        _map[aggregation] = field;
                    }
                    else
                    {
                        _map[aggregation] = aggregations[index];
                    }
                }

                // we want to visit this guy again to handle nested aggregations,
                // so we'll simply assign him to the old node and fall down to base.VisitGroup...
                node = new GroupExpression(
                    node.CorrelationId, // keep the same correlation id...
                    node.Type,
                    source,
                    node.Id,
                    aggregations.OfType<Expression>());
            }

            return base.VisitGroup(node);
        }

        protected override Expression VisitProject(ProjectExpression node)
        {
            // basically, find all the GroupedAggregateExpressions that exist before
            // me in the tree
            var oldLookup = _lookup;
            _lookup = new AggregateGatherer().Gather(node).ToLookup(x => x.GroupCorrelationId);

            // this VisitProject will corralate all these GroupedAggregateExpressions
            // and place them into the GroupExpression as well as replace
            // them here...
            var newNode = base.VisitProject(node);

            _lookup = oldLookup;

            // at this point, our _lookup fields are probably messed up cause any 
            // changes below have changed the references of our current groups in the lookup...
            // we need to update

            if(!(newNode is ProjectExpression))
            {
                return newNode;
            }

            node = (ProjectExpression)newNode;

            var groupAsSource = node.Source as GroupExpression;
            if (groupAsSource != null && groupAsSource.Aggregations.Count == 0)
            {
                // this conditional statement is used when a Distinct operation
                // has been rendered.  Distinct exists as a group of just _id.
                var keyMember = node.Projector as MemberExpression;
                if (keyMember != null &&
                    keyMember.Expression.Type.IsGenericType &&
                    keyMember.Expression.Type.GetGenericTypeDefinition() == typeof(Grouping<,>))
                {
                    return new GroupExpression(
                        groupAsSource.CorrelationId,
                        node.Projector.Type,
                        groupAsSource.Source,
                        groupAsSource.Id,
                        Enumerable.Empty<Expression>());
                }
            }
            return node;
        }

        // nested classes
        private class AggregateGatherer : LinqToMongoExpressionVisitor
        {
            private readonly List<GroupedAggregateExpression> _groupedAggregates;

            public AggregateGatherer()
            {
                _groupedAggregates = new List<GroupedAggregateExpression>();
            }

            public List<GroupedAggregateExpression> Gather(Expression node)
            {
                Visit(node);
                return _groupedAggregates;
            }

            protected override Expression VisitGroupedAggregate(GroupedAggregateExpression node)
            {
                _groupedAggregates.Add(node);

                // there may be nested grouped aggregates
                return base.VisitGroupedAggregate(node);
            }
        }

        // TODO: maybe...
        // Names for aggregations exist further up the tree than the GroupExpression.  
        // This gathers those names for us so we have a reference to them earlier.
        //private class AggregateNameGatherer : LinqToMongoExpressionVisitor
        //{
        //    private Dictionary<Expression, string> _names;

        //    public Dictionary<Expression, string> Gather(Expression node)
        //    {
        //        _names = new Dictionary<Expression, string>();
        //        Visit(node);
        //        return _names;
        //    }

        //    protected override NewExpression VisitNew(NewExpression node)
        //    {
        //        if (node.Members != null) // only do anonymous types
        //        {
        //            var properties = node.Type.GetProperties();
        //            var parameterToPropertyMap =
        //                from parameter in node.Constructor.GetParameters()
        //                join property in properties on parameter.Name equals property.Name
        //                select new { Parameter = parameter, Property = property };

        //            foreach (var parameterToProperty in parameterToPropertyMap)
        //            {
        //                var argument = node.Arguments[parameterToProperty.Parameter.Position];
        //                var agg = argument as AggregationExpression;
        //                if (agg != null)
        //                {
        //                    _names[agg] = parameterToProperty.Property.Name;
        //                }
        //            }
        //        }

        //        return node;
        //    }
        //}
    }
}