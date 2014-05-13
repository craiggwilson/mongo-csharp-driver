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
    [TestFixture, Category("Linq"), Category("Linq.Methods.OrderBy")]
    public class OrderByTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestOrderByAscending(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        orderby c.X
                        select c;

            var results = AssertQueryOrPipeline(query, "{ \"x\" : 1 }");

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results.First().X);
            Assert.AreEqual(5, results.Last().X);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestOrderByAscendingThenByAscending(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        orderby c.Y, c.X
                        select c;

            var results = AssertQueryOrPipeline(query, "{ \"y\" : 1, \"x\" : 1 }");

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results.First().X);
            Assert.AreEqual(5, results.Last().X);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestOrderByAscendingThenByDescending(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        orderby c.Y, c.X descending
                        select c;

            var results = AssertQueryOrPipeline(query, "{ \"y\" : 1, \"x\" : -1 }");

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(2, results.First().X);
            Assert.AreEqual(4, results.Last().X);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestOrderByDescending(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        orderby c.X descending
                        select c;

            var results = AssertQueryOrPipeline(query, "{ \"x\" : -1 }");

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(5, results.First().X);
            Assert.AreEqual(1, results.Last().X);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestOrderByDescendingThenByAscending(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        orderby c.Y descending, c.X
                        select c;

            var results = AssertQueryOrPipeline(query, "{ \"y\" : -1, \"x\" : 1 }");

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(4, results.First().X);
            Assert.AreEqual(2, results.Last().X);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestOrderByDescendingThenByDescending(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        orderby c.Y descending, c.X descending
                        select c;

            var results = AssertQueryOrPipeline(query, "{ \"y\" : -1, \"x\" : -1 }");

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(5, results.First().X);
            Assert.AreEqual(1, results.Last().X);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestOrderByDuplicate(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        orderby c.X
                        orderby c.Y
                        select c;

            var results = AssertQueryOrPipeline(query, "{ \"y\" : 1, \"x\" : 1 }");

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results.First().X);
            Assert.AreEqual(5, results.Last().X);
        }

        private List<T> AssertQueryOrPipeline<T>(IQueryable<T> query, string sortJson)
        {
            var model = GetExecutionModel(query);

            Assert.AreEqual(typeof(T), model.DocumentType);
            if (model.ModelType == ExecutionModelType.Query)
            {
                var queryModel = (QueryModel)model;
                Assert.IsNull(queryModel.Fields);
                Assert.IsNull(queryModel.NumberToLimit);
                Assert.IsNull(queryModel.NumberToSkip);
                Assert.IsNull(queryModel.Query);
                Assert.AreEqual(sortJson, queryModel.SortBy.ToJson());
            }
            else
            {
                var aggModel = (PipelineModel)model;
                Assert.AreEqual(1, aggModel.Pipeline.Count());
                Assert.AreEqual(aggModel.Pipeline.ElementAt(0)["$sort"].ToString(), sortJson);
            }

            return query.ToList();
        }
    }
}