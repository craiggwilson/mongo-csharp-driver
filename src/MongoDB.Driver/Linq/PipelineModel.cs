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
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Linq
{
    using System.Collections;

    /// <summary>
    /// Execution model that represents a query executed with the aggregation framework.
    /// </summary>
    public sealed class PipelineModel : ExecutionModel
    {
        // private fields
        private IEnumerable<BsonDocument> _pipeline;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineModel"/> class.
        /// </summary>
        public PipelineModel()
            : base(ExecutionModelType.Pipeline)
        {
        }

        // public properties
        /// <summary>
        /// Gets the pipeline.
        /// </summary>
        public IEnumerable<BsonDocument> Pipeline
        {
            get { return _pipeline; }
            set { _pipeline = value; }
        }

        // public methods
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var array = new BsonArray(_pipeline);
            return "aggregate(" + array.ToString() + ")";
        }

        /// <summary>
        /// Executes against the given collection
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <returns>
        /// Pipeline results
        /// </returns>
        public override object Execute(MongoCollection collection)
        {
            var args = new AggregateArgs { OutputMode = AggregateOutputMode.Cursor, Pipeline = this._pipeline };
            return this.Execute(collection, args);
        }

        /// <summary>
        /// Executes against the given collection with the supplied arguments
        /// </summary>
        /// <param name="collection">
        /// The collection.
        /// </param>
        /// <param name="args">
        /// The aggregate arguments.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public object Execute(MongoCollection collection, AggregateArgs args)
        {
            IBsonSerializer resultSerializer = null;
            IEnumerable results;
            if (this.Projection.Projector.Parameters[0].Type == typeof(IProjectionValueStore))
            {
                resultSerializer = new ProjectionValueStoreDeserializer(this.Projection.FieldSerializationInfo);
                results = collection.Aggregate<IProjectionValueStore>(args, resultSerializer);
            }
            else
            {
                resultSerializer = BsonSerializer.LookupSerializer(this.Projection.Projector.Parameters[0].Type);
                results = collection.Aggregate<object>(args, resultSerializer);
            }

            try
            {
                return this.CreateExecutor().Compile().DynamicInvoke(results);
            }
            catch (TargetInvocationException tie)
            {
                // a reflection exception occurs in this case 
                // if a database result doesn't conform to a linq to objects
                // result.  For instance, if Max() is used, but no results are returned
                // we would get this exception with an inner exception relating to 
                // the actual exception coming from a .Single() operation.  We
                // want to throw that inner exception, not this one.
                if (tie.InnerException != null)
                {
                    throw tie.InnerException;
                }

                throw;
            }
        }
    }
}