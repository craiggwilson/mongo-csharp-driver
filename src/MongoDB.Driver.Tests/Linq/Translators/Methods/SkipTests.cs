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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Skip")]
    public class SkipTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSkip2(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c).Skip(2);

            AssertQueryOrPipeline(query, 3, 2);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The SkipWhile query operator is not supported.")]
        public void TestSkipWhile(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target)
                .SkipWhile(c => true);
            query.ToList(); // execute query
        }

        private void AssertQueryOrPipeline<T>(IQueryable<T> query, int count, int skip)
        {
            var model = GetExecutionModel(query);

            Assert.AreEqual(typeof(T), model.DocumentType);
            if (model.ModelType == ExecutionModelType.Query)
            {
                var queryModel = (QueryModel)model;
                Assert.IsNull(queryModel.Fields);
                Assert.IsNull(queryModel.NumberToLimit);
                Assert.AreEqual(skip, queryModel.NumberToSkip);
                Assert.IsNull(queryModel.SortBy);
            }
            else
            {
                var aggModel = (PipelineModel)model;
                Assert.AreEqual(1, aggModel.Pipeline.Count());
                Assert.AreEqual(aggModel.Pipeline.ElementAt(0)["$skip"].ToInt32(), skip);
            }

            Assert.AreEqual(count, query.ToList().Count);
        }
    }
}