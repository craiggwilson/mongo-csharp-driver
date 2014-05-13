using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Driver.Tests.Linq.Translators
{
    [TestFixture, Category("Linq")]
    public class PipelineModelBuilderTests : LinqIntegrationTestBase
    {
        [Test]
        public void TestAnonymousProjectedFieldInSortClause()
        {
            var query = CreateQueryable<C>()
                .Select(x => new { Computed = x.X + x.Y })
                .OrderBy(x => x.Computed);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"Computed\" : { \"$add\" : [\"$x\", \"$y\"] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"Computed\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(12, results[0].Computed);
            Assert.AreEqual(13, results[1].Computed);
            Assert.AreEqual(36, results[2].Computed);
            Assert.AreEqual(48, results[3].Computed);
            Assert.AreEqual(49, results[4].Computed);
        }

        [Test]
        public void TestAnonymousProjectedFieldInWhereClause()
        {
            var query = CreateQueryable<C>()
                .Select(x => new { Computed = x.X + x.Y })
                .Where(x => x.Computed < 36);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"Computed\" : { \"$add\" : [\"$x\", \"$y\"] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"Computed\" : { \"$lt\" : 36 } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(13, results[0].Computed);
            Assert.AreEqual(12, results[1].Computed);
        }

        [Test]
        public void TestUnnamedComputedFieldInWhereClause()
        {
            var query = CreateQueryable<C>()
                .Select(x => x.X + x.Y)
                .Where(x => x < 36);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$add\" : [\"$x\", \"$y\"] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : { \"$lt\" : 36 } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(13, results[0]);
            Assert.AreEqual(12, results[1]);
        }

        [Test]
        public void TestNamedComputedFieldInWhereClause()
        {
            var query = CreateQueryable<C>()
                .Select(x => new { XPlusY = x.X + x.Y, X = x.X, Y = x.Y })
                .Where(x => x.XPlusY < 36)
                .Select(x => new { x.X, x.Y });

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"XPlusY\" : { \"$add\" : [\"$x\", \"$y\"] }, \"x\" : 1, \"y\" : 1, \"_id\" : 0 } }",
                "{ \"$match\" : { \"XPlusY\" : { \"$lt\" : 36 } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(2, results[0].X);
            Assert.AreEqual(1, results[1].X);
        }

        [Test]
        public void TestOrderByThenByWhereSkipTake()
        {
            var query = CreateQueryable<C>()
                .Select(x => new { XPlusY = x.X + x.Y, S = x.S, B = x.B, DZ = x.D.Z })
                .OrderBy(x => x.B)
                .ThenBy(x => x.S)
                .Where(x => x.XPlusY < 100)
                .Skip(1)
                .Take(2);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"XPlusY\" : { \"$add\" : [\"$x\", \"$y\"] }, \"s\" : 1, \"b\" : 1, \"d.z\" : 1, \"_id\" : 0 } }",
                "{ \"$match\" : { \"XPlusY\" : { \"$lt\" : 100 } } }",
                "{ \"$sort\" : { \"b\" : 1, \"s\" : 1 } }",
                "{ \"$skip\" : 1 }",
                "{ \"$limit\" : 2 }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(49, results[0].XPlusY);
            Assert.AreEqual(48, results[1].XPlusY);
        }
       
    }
}