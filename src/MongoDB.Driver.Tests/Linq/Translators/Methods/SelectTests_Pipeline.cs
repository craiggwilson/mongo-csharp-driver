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
    public class SelectTests_Pipeline : LinqIntegrationTestBase
    {
        [Test]
        [ExpectedException(typeof(MongoLinqException))]
        public void TestComplexProjectionWithQuery()
        {
            var query = CreateQueryable<C>(ExecutionTarget.Query)
                .Select(c => c.X + c.Y)
                .OrderBy(c => c)
                .ToList();
        }

        [Test]
        public void TestSelectAdd()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.X + c.Y)
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$add\" : [\"$x\", \"$y\"] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(12, results[0]);
            Assert.AreEqual(13, results[1]);
        }

        [Test]
        public void TestSelectAddFlattened()
        {
            var query = CreateQueryable<C>()
                .Select(x => x.X + x.Y + x.LX)
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$add\" : [\"$x\", \"$y\", \"$lx\"] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(13, results[0]);
            Assert.AreEqual(15, results[1]);
        }

        [Test]
        public void TestSelectAddInNewDocument()
        {
            var query = CreateQueryable<C>()
                .Select(x => new { Sum = x.X + x.Y })
                .OrderBy(x => x.Sum);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"Sum\" : { \"$add\" : [\"$x\", \"$y\"] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"Sum\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(12, results[0].Sum);
            Assert.AreEqual(13, results[1].Sum);
        }

        [Test]
        public void TestSelectAddInNewDocumentFlattened()
        {
            var query = CreateQueryable<C>()
                .Select(x => new { Sum = x.X + x.Y + 3 })
                .OrderBy(x => x.Sum);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"Sum\" : { \"$add\" : [\"$x\", \"$y\", 3] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"Sum\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(15, results[0].Sum);
            Assert.AreEqual(16, results[1].Sum);
        }

        [Test]
        public void TestSelectAnd()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.B && c.X == 3)
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$and\" : [\"$b\", { \"$eq\" : [\"$x\", 3] }] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");

            var results = query.ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectAndFlattened()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.B && c.X == 3 && c.Y <= 33)
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$and\" : [\"$b\", { \"$eq\" : [\"$x\", 3] }, { \"$lte\" : [\"$y\", 33] }] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");

            var results = query.ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectCoalesce()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.S ?? "zzz")
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$ifNull\" : [\"$s\", \"zzz\"] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual("   xyz   ", results[0]);
            Assert.AreEqual("zzz", results[4]);
        }

        [Test]
        public void TestSelectConcat()
        {
            var query = CreateQueryable<C>()
                .Select(x => x.S + x.S)
                .Where(x => x == "abcabc");

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$concat\" : [\"$s\", \"$s\"] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : \"abcabc\" } }");

            var results = query.ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("abcabc", results[0]);
        }

        [Test]
        public void TestSelectConcatFlattened()
        {
            var query = CreateQueryable<C>()
                .Select(x => x.S + " " + "Hi" + " " + x.S)
                .Where(x => x == "abc Hi abc");

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$concat\" : [\"$s\", \" \", \"Hi\", \" \", \"$s\"] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : \"abc Hi abc\" } }");

            var results = query.ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("abc Hi abc", results[0]);
        }

        [Test]
        public void TestSelectCondition()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.B ? 21 : 42)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$cond\" : [\"$b\", 21, 42] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(21, results[0]);
            Assert.AreEqual(42, results[4]);
        }

        [Test]
        public void TestSelectConditionDocument()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.X < 3 ? 21 : 42)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$cond\" : [{ \"$lt\" : [\"$x\", 3] }, 21, 42] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(21, results[0]);
            Assert.AreEqual(42, results[2]);
        }

        [Test]
        public void TestSelectDayOfMonth()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Date.Day)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$dayOfMonth\" : \"$date\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(3, results[4]);
        }

        [Test]
        public void TestSelectDayOfWeek()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Date.DayOfWeek)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$dayOfWeek\" : \"$date\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(DayOfWeek.Monday, results[0]);
            Assert.AreEqual(DayOfWeek.Saturday, results[4]);
        }

        [Test]
        public void TestSelectDayOfYear()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Date.DayOfYear)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$dayOfYear\" : \"$date\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(62, results[4]);
        }

        [Test]
        public void TestSelectDivide()
        {
            var query = CreateQueryable<C>()
                .Select(c => (double)c.Y / c.X)
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$divide\" : [\"$y\", \"$x\"] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(11d / 2, results[0]);
            Assert.AreEqual(44d / 5, results[1]);
        }

        [Test]
        public void TestSelectDivide3Numbers()
        {
            var query = CreateQueryable<C>()
                .Select(c => (double)c.Y / c.X / c.LX)
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$divide\" : [{ \"$divide\" : [\"$y\", \"$x\"] }, \"$lx\"] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(44d / 5 / 5, results[0]);
            Assert.AreEqual(44d / 4 / 4, results[1]);
        }

        [Test]
        public void TestSelectEquality()
        {
            var query = CreateQueryable<C>()
                .Select(x => x.S == "abc")
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$eq\" : [\"$s\", \"abc\"] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");

            var results = query.ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectGreaterThan()
        {
            var query = CreateQueryable<C>()
                .Select(x => x.X > 3)
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$gt\" : [\"$x\", 3] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectGreaterThanOrEqual()
        {
            var query = CreateQueryable<C>()
                .Select(x => x.X >= 3)
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$gte\" : [\"$x\", 3] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");

            var results = query.ToList();
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectHour()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Date.Hour)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$hour\" : \"$date\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(3, results[4]);
        }

        [Test]
        public void TestSelectLessThan()
        {
            var query = CreateQueryable<C>()
                .Select(x => x.X < 3)
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$lt\" : [\"$x\", 3] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectLessThanOrEqual()
        {
            var query = CreateQueryable<C>()
                .Select(x => x.X <= 3)
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$lte\" : [\"$x\", 3] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");

            var results = query.ToList();
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectMillisecond()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Date.Millisecond)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$millisecond\" : \"$date\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(3, results[4]);
        }

        [Test]
        public void TestSelectMinute()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Date.Minute)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$minute\" : \"$date\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(3, results[4]);
        }

        [Test]
        public void TestSelectModulo()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.X % 2)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$mod\" : [\"$x\", 2] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(0, results[0]);
            Assert.AreEqual(1, results[4]);
        }

        [Test]
        public void TestSelectMonth()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Date.Month)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$month\" : \"$date\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(3, results[4]);
        }

        [Test]
        public void TestSelectMultiply()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.X * c.Y)
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$multiply\" : [\"$x\", \"$y\"] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(11, results[0]);
            Assert.AreEqual(22, results[1]);
        }

        [Test]
        public void TestSelectMultiplyFlattened()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.X * c.Y * c.LX)
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$multiply\" : [\"$x\", \"$y\", \"$lx\"] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(11, results[0]);
            Assert.AreEqual(44, results[1]);
        }

        [Test]
        public void TestSelectNot()
        {
            var query = CreateQueryable<C>()
                .Select(x => !x.B)
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$not\" : \"$b\" }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");

            var results = query.ToList();
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectNotWithComparison()
        {
            var query = CreateQueryable<C>()
                .Select(x => !(x.X < 3))
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$not\" : [{ \"$lt\" : [\"$x\", 3] }] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");

            var results = query.ToList();
            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectNotEqual()
        {
            var query = CreateQueryable<C>()
                .Select(x => x.X != 3)
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$ne\" : [\"$x\", 3] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");

            var results = query.ToList();
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectOr()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.B || c.X == 1)
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$or\" : [\"$b\", { \"$eq\" : [\"$x\", 1] }] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectOrFlattened()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.B || c.X == 5 || c.Y <= 11)
                .Where(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$or\" : [\"$b\", { \"$eq\" : [\"$x\", 5] }, { \"$lte\" : [\"$y\", 11] }] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : true } }");
            
            var results = query.ToList();
            Assert.AreEqual(4, results.Count);
            Assert.AreEqual(true, results[0]);
        }

        [Test]
        public void TestSelectSecond()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Date.Second)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$second\" : \"$date\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results[0]);
            Assert.AreEqual(3, results[4]);
        }

        [Test]
        public void TestSelectSubstring()
        {
            var query = CreateQueryable<C>()
                .Where(c => c.S != null)
                .Select(c => c.S.Substring(2,2))
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$match\" : { \"s\" : { \"$ne\" : null } } }",
                "{ \"$project\" : { \"_fld0\" : { \"$substr\" : [\"$s\", 2, 2] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(" x", results[0]);
            Assert.AreEqual("c", results[1]);
        }

        [Test]
        public void TestSelectSubtract()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Y - c.X)
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$subtract\" : [\"$y\", \"$x\"] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(9, results[0]);
            Assert.AreEqual(10, results[1]);
        }

        [Test]
        public void TestSelectSubtract3Numbers()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Y - c.X - 3)
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$subtract\" : [{ \"$subtract\" : [\"$y\", \"$x\"] }, 3] }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(6, results[0]);
            Assert.AreEqual(7, results[1]);
        }

        [Test] 
        public void TestSelectToLower()
        {
            var query = CreateQueryable<C>()
                .Where(c => c.S != null)
                .Select(c => c.S.ToLower())
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$match\" : { \"s\" : { \"$ne\" : null } } }",
                "{ \"$project\" : { \"_fld0\" : { \"$toLower\" : \"$s\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("   xyz   ", results[0]);
            Assert.AreEqual("abc", results[1]);
        }

        [Test]
        public void TestSelectToLowerInvariant()
        {
            var query = CreateQueryable<C>()
                .Where(c => c.S != null)
                .Select(c => c.S.ToLowerInvariant())
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$match\" : { \"s\" : { \"$ne\" : null } } }",
                "{ \"$project\" : { \"_fld0\" : { \"$toLower\" : \"$s\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("   xyz   ", results[0]);
            Assert.AreEqual("abc", results[1]);
        }

        [Test]
        public void TestSelectToUpper()
        {
            var query = CreateQueryable<C>()
                .Where(c => c.S != null)
                .Select(c => c.S.ToUpper())
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$match\" : { \"s\" : { \"$ne\" : null } } }",
                "{ \"$project\" : { \"_fld0\" : { \"$toUpper\" : \"$s\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("   XYZ   ", results[0]);
            Assert.AreEqual("ABC", results[1]);
        }

        [Test]
        public void TestSelectToUpperInvariant()
        {
            var query = CreateQueryable<C>()
                .Where(c => c.S != null)
                .Select(c => c.S.ToUpperInvariant())
                .OrderBy(c => c);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$match\" : { \"s\" : { \"$ne\" : null } } }",
                "{ \"$project\" : { \"_fld0\" : { \"$toUpper\" : \"$s\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("   XYZ   ", results[0]);
            Assert.AreEqual("ABC", results[1]);
        }

        [Test]
        public void TestSelectYear()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Date.Year)
                .OrderBy(x => x);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$year\" : \"$date\" }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"_fld0\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(2000, results[0]);
            Assert.AreEqual(2001, results[4]);
        }

        [Test]
        public void TestSelectXPlusYAndDY()
        {
            var query = CreateQueryable<C>()
                .Select(x => new { XPlusY = x.X + x.Y, DY = x.D.Y })
                .Where(x => x.XPlusY < 36);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"XPlusY\" : { \"$add\" : [\"$x\", \"$y\"] }, \"d.y\" : 1, \"_id\" : 0 } }",
                "{ \"$match\" : { \"XPlusY\" : { \"$lt\" : 36 } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(13, results[0].XPlusY);
            Assert.AreEqual(12, results[1].XPlusY);
        }

        [Test]
        public void TestSelectWithSingleElementArrayProjection()
        {
            var query = CreateQueryable<C>()
                .Select(x => new { XPlusY = x.X + x.Y, Z = x.DA.Select(da => da.Z) })
                .Where(x => x.XPlusY < 36);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"XPlusY\" : { \"$add\" : [\"$x\", \"$y\"] }, \"Z\" : \"$da.z\", \"_id\" : 0 } }",
                "{ \"$match\" : { \"XPlusY\" : { \"$lt\" : 36 } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(13, results[0].XPlusY);
            Assert.AreEqual(12, results[1].XPlusY);
        }

        [Test]
        [Ignore("MongoDB does weird things with this result.  Let's deal with this later.")]
        public void TestSelectWithComplexElementArrayProjection()
        {
            var query = CreateQueryable<C>()
                .Select(x => new { XPlusY = x.X + x.Y, Zs = x.DA.Select(da => new { da.Y, da.Z }) })
                .Where(x => x.XPlusY < 36);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"XPlusY\" : { \"$add\" : [\"$x\", \"$y\"] }, \"Zs\" : { \"Y\" : \"$da.y\", \"Z\" : \"$da.z\" }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"XPlusY\" : { \"$lt\" : 36 } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(13, results[0].XPlusY);
            Assert.AreEqual(12, results[1].XPlusY);
        }

        [Test]
        public void TestSelectWithFinalProjection()
        {
            var query = CreateQueryable<C>()
                .Select(c => c.Y * 2)
                .Where(y => y < 40)
                .Select(y => new { Value = y / 2 });

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"_fld0\" : { \"$multiply\" : [\"$y\", 2] }, \"_id\" : 0 } }",
                "{ \"$match\" : { \"_fld0\" : { \"$lt\" : 40 } } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual(11, results[0].Value);
            Assert.AreEqual(11, results[1].Value);
        }

        [Test]
        public void TestSelectWithNestedAnonymousClassesOrderedByProjectedField()
        {
            var query = CreateQueryable<C>()
                .Select(c => new { Sum = new { First = c.X, Second = c.Y + c.LX } })
                .OrderBy(c => c.Sum.Second);

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$project\" : { \"Sum\" : { \"First\" : \"$x\", \"Second\" : { \"$add\" : [\"$y\", \"$lx\"] } }, \"_id\" : 0 } }",
                "{ \"$sort\" : { \"Sum.Second\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(5, results.Count);
            Assert.AreEqual(1, results[0].Sum.First);
            Assert.AreEqual(12, results[0].Sum.Second);
            Assert.AreEqual(5, results[4].Sum.First);
            Assert.AreEqual(49, results[4].Sum.Second);
        }
    }
}