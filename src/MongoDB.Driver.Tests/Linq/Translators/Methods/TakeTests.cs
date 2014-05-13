using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MongoDB.Driver.Tests.Linq.Translators.Methods
{
    [TestFixture, Category("Linq"), Category("Linq.Methods.Take")]
    public class TakeTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestTake2(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c).Take(2);

            AssertQueryOrPipeline(query, 2, 2);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The TakeWhile query operator is not supported.")]
        public void TestTakeWhile(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target)
                .TakeWhile(c => true);
            query.ToList(); // execute query
        }

        private void AssertQueryOrPipeline<T>(IQueryable<T> query, int count, int take)
        {
            var model = GetExecutionModel(query);

            Assert.AreEqual(typeof(T), model.DocumentType);
            if (model.ModelType == ExecutionModelType.Query)
            {
                var queryModel = (QueryModel)model;
                Assert.IsNull(queryModel.Fields);
                Assert.AreEqual(take, queryModel.NumberToLimit);
                Assert.IsNull(queryModel.NumberToSkip);
                Assert.IsNull(queryModel.SortBy);
            }
            else
            {
                var aggModel = (PipelineModel)model;
                Assert.AreEqual(1, aggModel.Pipeline.Count());
                Assert.AreEqual(aggModel.Pipeline.ElementAt(0)["$limit"].ToInt32(), take);
            }

            Assert.AreEqual(count, query.ToList().Count);
        }
    }
}