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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace MongoDB.Driver.Linq.Expressions
{
    [DebuggerTypeProxy(typeof(SortExpressionDebugView))]
    [DebuggerDisplay("{ToString()}")]
    internal class SortExpression : LinqToMongoExpression
    {
        // private fields
        private readonly ReadOnlyCollection<SortClause> _sortClauses;
        private readonly Expression _source;

        // constructors
        public SortExpression(Expression source, IEnumerable<SortClause> sortClauses)
            : base(LinqToMongoExpressionType.Sort, source.Type)
        {
            _sortClauses = sortClauses as ReadOnlyCollection<SortClause>;
            if (_sortClauses == null && sortClauses != null)
            {
                _sortClauses = new List<SortClause>(sortClauses).AsReadOnly();
            }
            _source = source;
        }

        // public properties
        public ReadOnlyCollection<SortClause> SortClauses
        {
            get { return _sortClauses; }
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

    internal class SortExpressionDebugView
    {
        private readonly SortExpression _node;

        public SortExpressionDebugView(SortExpression node)
        {
            _node = node;
        }

        public string Fields
        {
            get
            {
                return string.Join(", ",
                    _node.SortClauses.Select(x =>
                        string.Format("{0} {1}",
                        x.Expression is FieldExpression ? ((FieldExpression)x.Expression).SerializationInfo.ElementName : x.ToString(),
                        x.Direction)).ToArray());
            }
        }

        public Expression Source
        {
            get { return _node.Source; }
        }

        public override string ToString()
        {
            return string.Format("Sort({0})", Fields);
        }
    }
}