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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Select")]
    public class SelectTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelect(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        select c;

            var results = AssertQueryOrPipeline(query, null, null);

            Assert.AreEqual(5, results.Count);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectDY(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        select c.D.Y;

            var results = AssertQueryOrPipeline(query, "{ \"d.y\" : 1, \"_id\" : 0 }", null);

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(null, results.First());
            Assert.AreEqual(null, results.Last());
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectDZ(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        select c.D.Z;

            var results = AssertQueryOrPipeline(query, "{ \"d.z\" : 1, \"_id\" : 0 }", null);

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(22, results.First());
            Assert.AreEqual(44, results.Last());
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectFGH(ExecutionTarget target)
        {
            // need to restrict F and F.G because the aggregation framework adds in null responses
            // for these when they don't exist
            var query = from c in CreateQueryable<C>(target)
                        where c.F != null && c.F.G != null
                        select c.F.G.H;

            var results = AssertQueryOrPipeline(query, 
                "{ \"f.g.h\" : 1, \"_id\" : 0 }",
                "{ \"f\" : { \"$ne\" : null }, \"f.g\" : { \"$ne\" : null } }");

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(10, results.Single());
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectX(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        select c.X;

            var results = AssertQueryOrPipeline(query, "{ \"x\" : 1, \"_id\" : 0 }", null);

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(2, results.First());
            Assert.AreEqual(4, results.Last());
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectXPlusY(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        select c.X + c.Y;

            var results = AssertQueryOrPipeline(query, "{ \"x\" : 1, \"y\" : 1, \"_id\" : 0 }", null);

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(13, results.First());
            Assert.AreEqual(48, results.Last());
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectXAndY(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        select new { c.X, c.Y };

            var results = AssertQueryOrPipeline(query, "{ \"x\" : 1, \"y\" : 1, \"_id\" : 0 }", null);

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(2, results.First().X);
            Assert.AreEqual(4, results.Last().X);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectFollowedByWhere(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c.D).Where(d => d.Z == 11);

            var results = AssertQueryOrPipeline(query, 
                "{ \"d\" : 1, \"_id\" : 0 }", 
                "{ \"d.z\" : 11 }");

            Assert.AreEqual(1, results.Count);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectWithArray(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        select new { c.DA };

            var results = AssertQueryOrPipeline(query, "{ \"da\" : 1, \"_id\" : 0 }", null);

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(2, results.First().DA.Count);
            Assert.AreEqual(2, results.Last().DA.Count);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectWithBinaryOperation(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        select new { XPlusY = c.X + c.Y };

            var results = AssertQueryOrPipeline(query, "{ \"x\" : 1, \"y\" : 1, \"_id\" : 0 }", null);

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(13, results.First().XPlusY);
            Assert.AreEqual(48, results.Last().XPlusY);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectWithHierarchicalDocumentFieldSelection(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        select new { c.X, c.D, Q = new { c.D.Z } };

            var results = AssertQueryOrPipeline(query, "{ \"d\" : 1, \"x\" : 1, \"_id\" : 0 }", null);

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(22, results.First().Q.Z);
            Assert.AreEqual(44, results.Last().Q.Z);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException))]
        public void TestSelectWithIndex(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target)
                .Select((c, index) => c);

            query.ToList(); // execute query
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectWithRepeatedField(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        select new { c.X, c.Y, Q = new { c.X, c.D } };

            var results = AssertQueryOrPipeline(query, "{ \"d\" : 1, \"x\" : 1, \"y\" : 1, \"_id\" : 0 }", null);

            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(2, results.First().Q.X);
            Assert.AreEqual(4, results.Last().Q.X);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectWithSingleElementArrayProjection(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.DA != null
                        select new { c.X, Z = c.DA.Select(x => x.Z) };

            var results = AssertQueryOrPipeline(query, 
                "{ \"da.z\" : 1, \"x\" : 1, \"_id\" : 0 }",
                "{ \"da\" : { \"$ne\" : null } }");

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(2, results.First().Z.Count(), 2);
            Assert.AreEqual(2, results.Last().Z.Count(), 4);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectWithComplexElementArrayProjection(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.DA != null
                        select new { c.X, Zs = c.DA.Select(x => new { x.Y, x.Z }).ToList() };

            var results = AssertQueryOrPipeline(query,
                "{ \"da.y\" : 1, \"da.z\" : 1, \"x\" : 1, \"_id\" : 0 }",
                "{ \"da\" : { \"$ne\" : null } }");

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(111, results.First().Zs[0].Z);
            Assert.AreEqual(444, results.Last().Zs[1].Z);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectWithComplexDuplicateElementArrayProjection(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.DA != null
                        select new { c.X, Zs = c.DA.Select(x => x.Z).ToList(), Others = c.DA.Select(x => new { x.Y }).ToList() };
            
            var results = AssertQueryOrPipeline(query,
                "{ \"da.y\" : 1, \"da.z\" : 1, \"x\" : 1, \"_id\" : 0 }",
                "{ \"da\" : { \"$ne\" : null } }");

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(111, results.First().Zs[0]);
            Assert.AreEqual(444, results.Last().Zs[1]);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectWithComplexMixingOfArrayElementAndParentElement(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.DA != null
                        select new { c.X, Zs = c.DA.Select(x => new { c.B, x.Y }) };

            var results = AssertQueryOrPipeline(query,
                "{ \"b\" : 1, \"da.y\" : 1, \"x\" : 1, \"_id\" : 0 }",
                "{ \"da\" : { \"$ne\" : null } }");

            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(false, results.First().Zs.First().B);
            Assert.AreEqual(false, results.Last().Zs.Last().B);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectXPlusYAndDZ(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target)
                .Select(x => new { XPlusY = x.X + x.Y, DZ = x.D.Z });

            var results = AssertQueryOrPipeline(query, "{ \"d.z\" : 1, \"x\" : 1, \"y\" : 1, \"_id\" : 0 }", null);

            Assert.AreEqual(5, results.Count);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSelectWithNestedAnonymousClassesOrderedByNonProjectedField(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target)
                .Select(c => new { Sum = new { First = c.X, Second = c.Y + c.LX }, Str = c.S })
                .OrderBy(c => c.Str);

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(2, results[0].Sum.First);
            Assert.AreEqual(13, results[0].Sum.Second);
            Assert.AreEqual(1, results[4].Sum.First);
            Assert.AreEqual(12, results[4].Sum.Second);
        }

        private List<T> AssertQueryOrPipeline<T>(IQueryable<T> query, string projectJson, string queryJson)
        {
            var model = GetExecutionModel(query);

            if (model.ModelType == ExecutionModelType.Query)
            {
                var queryModel = (QueryModel)model;
                Assert.IsNull(queryModel.NumberToLimit);
                Assert.IsNull(queryModel.NumberToSkip);
                Assert.IsNull(queryModel.SortBy);
                if(queryJson != null)
                {
                    Assert.AreEqual(queryJson, queryModel.Query.ToJson());
                }
                else
                {
                    Assert.IsNull(queryModel.Query);
                }

                if (projectJson != null)
                {
                    Assert.AreEqual(projectJson, queryModel.Fields.ToJson());
                }
                else
                {
                    Assert.IsNull(queryModel.Fields);
                }
            }
            else
            {
                var aggModel = (PipelineModel)model;
                int count = 0;
                if(queryJson != null)
                {
                    var match = aggModel.Pipeline.Single(x => x.GetElement(0).Name == "$match");
                    Assert.AreEqual(queryJson, match.Single().Value.ToString());
                    count++;
                }
                if (projectJson != null)
                {
                    var project = aggModel.Pipeline.Single(x => x.GetElement(0).Name == "$project");
                    Assert.AreEqual(projectJson, project.Single().Value.ToString());
                    count++;
                }
                    Assert.AreEqual(count, aggModel.Pipeline.Count());
            }

            return query.ToList();
        }
    }
}