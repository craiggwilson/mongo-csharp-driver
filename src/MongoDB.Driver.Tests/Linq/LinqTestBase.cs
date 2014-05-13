using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MongoDB.Driver.Tests.Linq
{
    public abstract class LinqTestBase
    {
        protected IQueryable<T> CreateQueryable<T>(MongoCollection collection)
        {
            return CreateQueryable<T>(collection, ExecutionTarget.Best);
        }

        protected IQueryable<T> CreateQueryable<T>(MongoCollection collection, ExecutionTarget target)
        {
            return new LinqToMongoQueryable<T>(new LinqToMongoQueryProvider(collection, target));
        }

        protected PipelineModel GetPipelineModel<T>(IQueryable<T> query)
        {
            return (PipelineModel)GetExecutionModel(query);
        }

        protected QueryModel GetQueryModel<T>(IQueryable<T> query)
        {
            return (QueryModel)GetExecutionModel(query);
        }

        protected ExecutionModel GetExecutionModel<T>(IQueryable<T> query)
        {
            return ((LinqToMongoQueryable<T>)query).BuildQueryModel();
        }

        protected void AssertPipeline(IEnumerable<BsonDocument> pipeline, params string[] expectedJson)
        {
            var pipelineList = pipeline.ToList();
            Assert.AreEqual(expectedJson.Length, pipelineList.Count, "Pipeline count was incorrect.");
            for (int i = 0; i < pipelineList.Count; i++)
            {
                Assert.AreEqual(expectedJson[i], pipelineList[i].ToJson(), string.Format("Pipeline[{0}]", i));
            }
        }
    }
}