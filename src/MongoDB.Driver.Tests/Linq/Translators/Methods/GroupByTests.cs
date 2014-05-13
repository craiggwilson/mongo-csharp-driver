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
    [TestFixture, Category("Linq"), Category("Linq.Methods.GroupBy")]
    public class GroupByTests : LinqIntegrationTestBase
    {
        [Test]
        [ExpectedException(typeof(MongoLinqException))]
        public void TestGroupByWithAQuery()
        {
            var query = from c in CreateQueryable<C>(ExecutionTarget.Query)
                        group c by c.B into g
                        select g.Key;

            query.ToList();
        }

        [Test]
        public void TestGroupByBSelectKey()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c.B into g
                        select g.Key;

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : \"$b\" } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.Contains(true, results);
            Assert.Contains(false, results);
        }

        [Test]
        public void TestGroupByBSelectCount()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c.B into g
                        select g.Count();

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$sum\" : 1 } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.Contains(1, results);
            Assert.Contains(4, results);
        }

        [Test]
        public void TestGroupByBSelectAnonymousTypeKeyAndCount()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c.B into g
                        select new { g.Key, Count = g.Count() };

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$sum\" : 1 } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.Contains(new { Key = true, Count = 1 }, results);
            Assert.Contains(new { Key = false, Count = 4 }, results);
        }

        [Test]
        public void TestGroupByBOrderByKeySelectAnonymousTypeKeyAndCount()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c.B into g
                        orderby g.Key
                        select new { g.Key, Count = g.Count() };

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$sum\" : 1 } } }",
                "{ \"$sort\" : { \"_id\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(false, results[0].Key);
            Assert.AreEqual(4, results[0].Count);
            Assert.AreEqual(true, results[1].Key);
            Assert.AreEqual(1, results[1].Count);
        }

        [Test]
        public void TestGroupByBOrderByKeySelectKeyWhereCountEquals1()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c.B into g
                        orderby g.Key
                        where g.Count() == 4
                        select g.Key;

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$sum\" : 1 } } }",
                "{ \"$match\" : { \"_agg0\" : 4 } }",
                "{ \"$sort\" : { \"_id\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(false, results[0]);
        }

        [Test]
        public void TestGroupByAnonymousType()
        {
            var query = from c in CreateQueryable<C>()
                        group c by new { c.X, c.LX } into g
                        orderby g.Key.X
                        select new { g.Key.LX, Count = g.Count() };

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : { \"X\" : \"$x\", \"LX\" : \"$lx\" }, \"_agg0\" : { \"$sum\" : 1 } } }",
                "{ \"$sort\" : { \"_id.X\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results[0].LX);
            Assert.AreEqual(2, results[1].LX);
            Assert.AreEqual(3, results[2].LX);
            Assert.AreEqual(4, results[3].LX);
            Assert.AreEqual(5, results[4].LX);
        }

        [Test]
        public void TestGroupByBOrderByGroupCountDescendingSelectAnonymousTypeKeyAndCount()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c.B into g
                        orderby g.Count() descending
                        select new { g.Key, Count = g.Count() };

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$sum\" : 1 } } }",
                "{ \"$sort\" : { \"_agg0\" : -1 } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(false, results[0].Key);
            Assert.AreEqual(4, results[0].Count);
            Assert.AreEqual(true, results[1].Key);
            Assert.AreEqual(1, results[1].Count);
        }

        [Test]
        public void TestGroupByBSumXPlusY()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c.B into g
                        select g.Sum(x => x.X + x.Y);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$sum\" : { \"$add\" : [\"$x\", \"$y\"] } } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(36, results[0]);
            Assert.AreEqual(122, results[1]);
        }

        [Test]
        public void TestGroupByBMinAndMax()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c.B into g
                        select new { Min = g.Min(x => x.X), Max = g.Max(x => x.X) };

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$min\" : \"$x\" }, \"_agg1\" : { \"$max\" : \"$x\" } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(3, results[0].Min);
            Assert.AreEqual(3, results[0].Max);
            Assert.AreEqual(1, results[1].Min);
            Assert.AreEqual(5, results[1].Max);
        }

        [Test]
        public void TestGroupByBAverage()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c.B into g
                        select g.Average(x => x.X);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$avg\" : \"$x\" } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(3d, results[0]);
            Assert.AreEqual(3d, results[1]);
        }

        [Test]
        public void TestGroupByBFirstX1()
        {
            var query = from c in CreateQueryable<C>()
                        orderby c.B
                        group c by c.B into g
                        select g.Select(x => x.X).First();

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$sort\" : { \"b\" : 1 } }",
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$first\" : \"$x\" } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(3, results[0]);
            Assert.AreEqual(2, results[1]);
        }

        [Test]
        public void TestGroupByBFirstX2()
        {
            var query = from c in CreateQueryable<C>()
                        orderby c.B
                        group c by c.B into g
                        select g.First().X;

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$sort\" : { \"b\" : 1 } }",
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$first\" : \"$x\" } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(3, results[0]);
            Assert.AreEqual(2, results[1]);
        }

        [Test]
        public void TestGroupByBLastX1()
        {
            var query = from c in CreateQueryable<C>()
                        orderby c.B
                        group c by c.B into g
                        select g.Select(x => x.X).Last();

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$sort\" : { \"b\" : 1 } }",
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$last\" : \"$x\" } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(3, results[0]);
            Assert.AreEqual(4, results[1]);
        }

        [Test]
        public void TestGroupByBLastX2()
        {
            var query = from c in CreateQueryable<C>()
                        orderby c.B
                        group c by c.B into g
                        select g.Last().X;

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$sort\" : { \"b\" : 1 } }",
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$last\" : \"$x\" } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(3, results[0]);
            Assert.AreEqual(4, results[1]);
        }

        [Test]
        public void TestGroupByBLastFGH()
        {
            var query = from c in CreateQueryable<C>()
                        orderby c.B
                        group c by c.B into g
                        select g.Select(x => (int?)x.F.G.H).Last();

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$sort\" : { \"b\" : 1 } }",
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$last\" : \"$f.g.h\" } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.IsNull(results[0]);
            Assert.IsNull(results[1]);
        }

        [Test]
        public void TestGroupByBMinX()
        {
            var query = from c in CreateQueryable<C>()
                        orderby c.B
                        group c by c.B into g
                        select g.Select(x => x.X).Min();

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$sort\" : { \"b\" : 1 } }",
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$min\" : \"$x\" } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(3, results[0]);
            Assert.AreEqual(1, results[1]);
        }

        [Test]
        public void TestGroupByBIntoAnonymousTypeSumXPlusYAndCount()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c.B into g
                        select new { One = g.Sum(x => x.X + x.Y), Two = g.Count() };

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : \"$b\", \"_agg0\" : { \"$sum\" : { \"$add\" : [\"$x\", \"$y\"] } }, \"_agg1\" : { \"$sum\" : 1 } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(36, results[0].One);
            Assert.AreEqual(1, results[0].Two);
            Assert.AreEqual(122, results[1].One);
            Assert.AreEqual(4, results[1].Two);
        }

        [Test]
        public void TestGroupByEntireDocument()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c into g
                        select g;
            Assert.Throws<MongoLinqException>(() => query.ToList());
        }

        [Test]
        public void TestGroupBySingleElementWithoutProjections()
        {
            var query = from c in CreateQueryable<C>()
                        group c by c.B into g
                        select g;
            Assert.Throws<MongoLinqException>(() => query.ToList());
        }

        [Test]
        public void TestGroupByWithElementSelector()
        {
            var query = CreateQueryable<C>().GroupBy(c => c.B, c => c.B);
            Assert.Throws<MongoLinqException>(() => query.ToList());
        }

        [Test]
        public void TestGroupByWithKeySelectorAndEqualityComparer()
        {
            var query = CreateQueryable<C>().GroupBy(c => c, new CEqualityComparer());
            Assert.Throws<MongoLinqException>(() => query.ToList());
        }

        [Test]
        public void TestGroupByWithKeySelectorAndResultSelector()
        {
            // This should be possible...
            var query = (from c in CreateQueryable<C>()
                         select c).GroupBy(c => c, (k, e) => 1.0);

            Assert.Throws<MongoLinqException>(() => query.ToList());
        }

        [Test]
        public void TestGroupByWithKeySelectorAndResultSelectorAndEqualityComparer()
        {
            var query = (from c in CreateQueryable<C>()
                         select c).GroupBy(c => c, (k, e) => e.First(), new CEqualityComparer());
            Assert.Throws<MongoLinqException>(() => query.ToList());
        }
    }
}