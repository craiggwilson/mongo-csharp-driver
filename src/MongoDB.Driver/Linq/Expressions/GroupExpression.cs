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
using System.Diagnostics;
using System.Linq.Expressions;

namespace MongoDB.Driver.Linq.Expressions
{
    [DebuggerTypeProxy(typeof(GroupExpressionDebugView))]
    [DebuggerDisplay("{ToString()}")]
    internal class GroupExpression : LinqToMongoExpression
    {
        // private fields
        private readonly Guid _correlationId;
        private readonly Expression _id;
        private readonly ReadOnlyCollection<Expression> _aggregations;
        private readonly Expression _source;

        // constructors
        public GroupExpression(Guid correlationId, Type type, Expression source, Expression id, IEnumerable<Expression> aggregations)
            : base(LinqToMongoExpressionType.Group, type)
        {
            _correlationId = correlationId;
            _source = source;
            _id = id;
            _aggregations = aggregations as ReadOnlyCollection<Expression>;
            if (_aggregations == null && aggregations != null)
            {
                _aggregations = new List<Expression>(aggregations).AsReadOnly();
            }
        }

        // public properties
        public ReadOnlyCollection<Expression> Aggregations
        {
            get { return _aggregations; }
        }

        public Guid CorrelationId
        {
            get { return _correlationId; }
        }

        public Expression Id
        {
            get { return _id; }
        }

        public Expression Source
        {
            get { return _source; }
        }

        // public methods
        public override string ToString()
        {
            return LinqToMongoExpressionFormatter.ToString(this);
        }
    }

    internal class GroupExpressionDebugView
    {
        private readonly GroupExpression _node;

        public GroupExpressionDebugView(GroupExpression node)
        {
            _node = node;
        }

        public ReadOnlyCollection<Expression> Aggregations
        {
            get { return _node.Aggregations; }
        }

        public Expression Id
        {
            get { return _node.Id; }
        }

        public Expression Source
        {
            get { return _node.Source; }
        }

        public Type Type
        {
            get { return _node.Type; }
        }
    }
}