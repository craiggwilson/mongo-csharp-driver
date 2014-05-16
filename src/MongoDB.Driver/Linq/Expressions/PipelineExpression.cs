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
using System.Diagnostics;
using System.Linq.Expressions;

namespace MongoDB.Driver.Linq.Expressions
{
    /// <remarks>
    /// Only one of these will exist in a bound expression tree.  It will also generally
    /// exist as the highest node in a tree.
    /// </remarks>
    [DebuggerTypeProxy(typeof(PipelineExpressionDebugView))]
    [DebuggerDisplay("{ToString()}")]
    internal class PipelineExpression : LinqToMongoExpression
    {
        // private fields
        private readonly Expression _source;
        private readonly Expression _projector;
        private readonly LambdaExpression _aggregator;

        // constructors
        public PipelineExpression(Expression source, Expression projector)
            : this(source, projector, null)
        { }

        public PipelineExpression(Expression source, Expression projector, LambdaExpression aggregator)
            : base(LinqToMongoExpressionType.Pipeline, aggregator != null ? aggregator.Body.Type : typeof(IEnumerable<>).MakeGenericType(projector.Type))
        {
            _source = source;
            _projector = projector;
            _aggregator = aggregator;
        }

        // public properties
        public LambdaExpression Aggregator
        {
            get { return _aggregator; }
        }

        public Expression Projector
        {
            get { return _projector; }
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

    internal class PipelineExpressionDebugView
    {
        private readonly PipelineExpression _node;

        public PipelineExpressionDebugView(PipelineExpression node)
        {
            _node = node;
        }

        public LambdaExpression Aggregator
        {
            get { return _node.Aggregator; }
        }

        public Expression Projector
        {
            get { return _node.Projector; }
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
