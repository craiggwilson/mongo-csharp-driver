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

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Base class for the two types of execution models, <see cref="QueryModel"/> and <see cref="PipelineModel"/>.
    /// </summary>
    public abstract class ExecutionModel
    {
        // private fields
        private readonly ExecutionModelType _modelType;
        private LambdaExpression _aggregator;
        private Type _documentType;
        private ExecutionModelProjection _projection;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionModel" /> class.
        /// </summary>
        /// <param name="modelType">Type of the model.</param>
        /// <remarks>
        /// Internal to prevent 3rd party query models from getting created.  Only two
        /// subclasses are allowed, <see cref="QueryModel"/> and <see cref="PipelineModel"/>.
        /// </remarks>
        internal ExecutionModel(ExecutionModelType modelType)
        {
            _modelType = modelType;
        }

        // public properties
        /// <summary>
        /// Gets or sets the aggregator. This runs after all the documents have been projected.
        /// </summary>
        public LambdaExpression Aggregator
        {
            get { return _aggregator; }
            set { _aggregator = value; }
        }

        /// <summary>
        /// Gets or sets the type of document.
        /// </summary>
        public Type DocumentType
        {
            get { return _documentType; }
            set { _documentType = value; }
        }

        /// <summary>
        /// Gets the type of the model.
        /// </summary>
        public ExecutionModelType ModelType
        {
            get { return _modelType; }
        }

        // internal properties
        internal ExecutionModelProjection Projection
        {
            get { return _projection; }
            set { _projection = value; }
        }

        // internal methods
        internal abstract object Execute(MongoCollection collection);

        internal LambdaExpression CreateExecutor()
        {
            var documents = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(Projection.Projector.Parameters[0].Type), "all");
            
            // because we will always have a projector, Select is the natural operator
            // to apply that projection.
            // TODO: is there any way to avoid this - maybe if Projection.Projector
            // is an identity projector.
            Expression body = Expression.Call(
                typeof(Enumerable),
                "Select",
                new[] { Projection.Projector.Parameters[0].Type, Projection.Projector.Body.Type },
                documents,
                Projection.Projector);

            // the aggregator gets applied to the result once all documents have been projected.
            if (Aggregator != null)
            {
                body = Expression.Invoke(Aggregator, body);
            }

            // need to ensure conversion.
            if (body.Type != typeof(object))
            {
                body = Expression.Convert(body, typeof(object));
            }

            return Expression.Lambda(body, documents);
        }
    }
}