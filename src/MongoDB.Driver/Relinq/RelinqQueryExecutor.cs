/* Copyright 2010-2015 MongoDB Inc.
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Relinq.Structure.Expressions;
using MongoDB.Driver.Relinq.Language;
using MongoDB.Driver.Relinq.Preparation;
using MongoDB.Driver.Sync;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using MongoDB.Driver.Relinq.Preparation.Pipeline;

namespace MongoDB.Driver.Relinq
{
    internal interface IRelinqQueryExecutor : IQueryExecutor
    {
        RenderedPipelineDefinition<T> RenderPipeline<T>(QueryModel queryModel);
    }

    internal class RelinqQueryExecutor<TDocument> : IRelinqQueryExecutor
    {
        private readonly IMongoCollection<TDocument> _collection;

        public RelinqQueryExecutor(IMongoCollection<TDocument> collection)
        {
            _collection = Ensure.IsNotNull(collection, "collection");
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var renderedPipeline = RenderPipeline<T>(queryModel);

            var pipeline = new BsonDocumentStagePipelineDefinition<TDocument, T>(
                renderedPipeline.Documents,
                renderedPipeline.OutputSerializer);

            return new AsyncCursorEnumerableAdapter<T>(ct => (Task<IAsyncCursor<T>>)_collection.AggregateAsync(pipeline), CancellationToken.None);
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            throw new NotImplementedException();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            throw new NotImplementedException();
        }

        public RenderedPipelineDefinition<T> RenderPipeline<T>(QueryModel queryModel)
        {
            ComplexSelectClauseSubQueryCreatingQueryModelVisitor.Apply(queryModel);

            var pipeline = PipelineBuildingQueryModelVisitor.Prepare(
                queryModel,
                _collection.DocumentSerializer,
                _collection.Settings.SerializerRegistry);
            return PipelineLanguageTranslator.Translate<T>(pipeline);
        }
    }
}
