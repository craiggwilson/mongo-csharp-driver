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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Distinct")]
    public class DistinctTests : LinqIntegrationTestBase
    {
        [Test]
        public void TestDistinctASub0()
        {
            // Aggregation Framework cannot do groupings on indexes in an array...
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = (from c in CreateQueryable<C>(ExecutionTarget.Query)
                             select c.A[0]).Distinct();
                var results = query.ToList();
                Assert.AreEqual(1, results.Count);
                Assert.IsTrue(results.Contains(2));
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestDistinctB(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c.B).Distinct();
            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.Contains(false));
            Assert.IsTrue(results.Contains(true));
        }

        [Test]
        public void TestDistinctBASub0()
        {
            // Aggregation Framework cannot do groupings on indexes in an array...
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = (from c in CreateQueryable<C>(ExecutionTarget.Query)
                             select c.BA[0]).Distinct();
                var results = query.ToList();
                Assert.AreEqual(1, results.Count);
                Assert.IsTrue(results.Contains(true));
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestDistinctD(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c.D).Distinct();
            var results = query.ToList(); // execute query
            Assert.AreEqual(5, results.Count);
            Assert.IsTrue(results.Contains(new D { Z = 11 }));
            Assert.IsTrue(results.Contains(new D { Z = 22 }));
            Assert.IsTrue(results.Contains(new D { Z = 33 }));
            Assert.IsTrue(results.Contains(new D { Z = 44 }));
            Assert.IsTrue(results.Contains(new D { Z = 55 }));
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestDistinctDBRef(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c.DBRef).Distinct();
            var results = query.ToList();
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results.Contains(new MongoDBRef("db", "c", 1)));
        }

        [Test]
        public void TestDistinctDBRefDatabase()
        {
            // Aggregation framework cannot deal with fields containing "$" signs, such
            // as DBRefs.
            var query = (from c in CreateQueryable<C>(ExecutionTarget.Query)
                         select c.DBRef.DatabaseName).Distinct();
            var results = query.ToList();
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results.Contains("db"));
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestDistinctDZ(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c.D.Z).Distinct();
            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsTrue(results.Contains(11));
            Assert.IsTrue(results.Contains(22));
            Assert.IsTrue(results.Contains(33));
            Assert.IsTrue(results.Contains(44));
            Assert.IsTrue(results.Contains(55));
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestDistinctE(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c.E).Distinct();
            var results = query.ToList();
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results.Contains(E.A));
        }

        [Test]
        public void TestDistinctEASub0()
        {
            // Aggregation Framework cannot do groupings on indexes in an array...
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = (from c in CreateQueryable<C>(ExecutionTarget.Query)
                             select c.EA[0]).Distinct();
                var results = query.ToList();
                Assert.AreEqual(1, results.Count);
                Assert.IsTrue(results.Contains(E.A));
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestDistinctId(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c.Id).Distinct();
            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsTrue(results.Contains(_id1));
            Assert.IsTrue(results.Contains(_id2));
            Assert.IsTrue(results.Contains(_id3));
            Assert.IsTrue(results.Contains(_id4));
            Assert.IsTrue(results.Contains(_id5));
        }

        [Test]
        public void TestDistinctLSub0()
        {
            // Aggregation Framework cannot do groupings on indexes in an array...
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = (from c in CreateQueryable<C>(ExecutionTarget.Query)
                             select c.L[0]).Distinct();
                var results = query.ToList();
                Assert.AreEqual(1, results.Count);
                Assert.IsTrue(results.Contains(2));
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestDistinctS(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c.S).Distinct();
            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.Contains("abc"));
            Assert.IsTrue(results.Contains("   xyz   "));
        }

        [Test]
        public void TestDistinctSASub0()
        {
            // Aggregation Framework cannot do groupings on indexes in an array...
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = (from c in CreateQueryable<C>(ExecutionTarget.Query)
                             select c.SA[0]).Distinct();
                var results = query.ToList();
                Assert.AreEqual(1, results.Count);
                Assert.IsTrue(results.Contains("Tom"));
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestDistinctX(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c.X).Distinct();
            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsTrue(results.Contains(1));
            Assert.IsTrue(results.Contains(2));
            Assert.IsTrue(results.Contains(3));
            Assert.IsTrue(results.Contains(4));
            Assert.IsTrue(results.Contains(5));
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestDistinctXWithQuery(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         where c.X > 3
                         select c.X).Distinct();
            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.Contains(4));
            Assert.IsTrue(results.Contains(5));
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestDistinctY(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c.Y).Distinct();
            var results = query.ToList();
            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results.Contains(11));
            Assert.IsTrue(results.Contains(33));
            Assert.IsTrue(results.Contains(44));
        }

        [Test]
        [ExpectedException(typeof(MongoLinqException))]
        public void TestDistinctWithEqualityComparer()
        {
            var query = CreateQueryable<C>().Distinct(new CEqualityComparer());
            query.ToList(); // execute query
        }

        [Test]
        public void TestDistinctYOrderByY()
        {
            var query = _collection.AsQueryable(ExecutionTarget.Pipeline)
                .Select(d => d.Y)
                .Distinct()
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"y\" : 1, \"_id\" : 0 } }",
                "{ \"$match\" : { \"y\" : { \"$exists\" : true } } }",
                "{ \"$group\" : { \"_id\" : \"$y\" } }",
                "{ \"$sort\" : { \"_id\" : 1 } }");

            var result = query.ToList();
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void TestDistinctXandYOrderByXAndY()
        {
            var query = _collection.AsQueryable(ExecutionTarget.Pipeline)
                .Select(d => new { d.X, d.Y })
                .Distinct()
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"x\" : 1, \"y\" : 1, \"_id\" : 0 } }",
                "{ \"$group\" : { \"_id\" : { \"X\" : \"$x\", \"Y\" : \"$y\" } } }",
                "{ \"$sort\" : { \"_id\" : 1 } }");

            var result = query.ToList();
            Assert.AreEqual(5, result.Count);
        }

        [Test]
        public void TestDistinctXandYOrderByY()
        {
            var query = _collection.AsQueryable(ExecutionTarget.Pipeline)
                .Select(d => new { d.X, d.Y })
                .Distinct()
                .OrderBy(x => x.Y);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"x\" : 1, \"y\" : 1, \"_id\" : 0 } }",
                "{ \"$group\" : { \"_id\" : { \"X\" : \"$x\", \"Y\" : \"$y\" } } }",
                "{ \"$sort\" : { \"_id.Y\" : 1 } }");

            var result = query.ToList();
            Assert.AreEqual(5, result.Count);
        }

        [Test]
        public void TestDistinctXandYFollowedByClientSideProjection()
        {
            var query = _collection.AsQueryable(ExecutionTarget.Pipeline)
                .Select(d => new { d.X, d.Y })
                .Distinct()
                .Select(x => x.X + x.Y);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"x\" : 1, \"y\" : 1, \"_id\" : 0 } }",
                "{ \"$group\" : { \"_id\" : { \"X\" : \"$x\", \"Y\" : \"$y\" } } }");

            var result = query.ToList();
            Assert.AreEqual(5, result.Count);
        }
    }
}