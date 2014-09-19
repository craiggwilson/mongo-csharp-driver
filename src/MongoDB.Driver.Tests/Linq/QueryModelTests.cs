using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MongoDB.Driver.Tests.Linq
{
    using MongoDB.Driver.Linq;

    [TestFixture]
    [Category("Linq")]
    public class QueryModelTests : LinqTestBase
    {
        private class B
        {
            public int I { get; set; }
        }

        private class A
        {
            public int I { get; set; }

            public bool B { get; set; }

            public string S { get; set; }

            public int[] AI { get; set; }

            public List<B> BL { get; set; }
        }

        [Test]
        public void TestToStringQueryProjectSkipLimit()
        {
            var query = (from a in CreateQueryable<A>(Configuration.TestCollection)
                         where a.I > 10
                         select new { C = a.B && a.S.StartsWith("funny") })
                        .Skip(10).Take(2);

            var s = query.ToString();
            Assert.AreEqual("find({ \"I\" : { \"$gt\" : 10 } }, { \"B\" : 1, \"S\" : 1, \"_id\" : 0 }).skip(10).limit(2)", s);
        }

        [Test]
        public void TestToStringDistinct()
        {
            var query = CreateQueryable<A>(Configuration.TestCollection).Select(x => x.I).Distinct();

            var s = query.ToString();
            Assert.AreEqual("distinct(\"I\")", s);
        }

        [Test]
        public void TestBsonDocumentBackedClass_LooselyTyped_Member()
        {
            // This test can be made to pass/fail by changing the register member call in mongodocumentclassserializer
            // from BooleanSerializer to BsonValueSerializer
            var mongoDocs = Configuration.TestDatabase.GetCollection<MongoDocument>("mongoDocs");
            var query = CreateQueryable<MongoDocument>(mongoDocs, ExecutionTarget.Query);

            query = query.Where(o => o["IsArchived"] == false);

            var model = this.GetQueryModel(query);
        }

        [Test]
        public void TestBsonDocumentBackedClass_StronglyTyped_Member()
        {
            // This test can be made to pass/fail by changing the register member call in mongodocumentclassserializer
            // from BooleanSerializer to BsonValueSerializer
            var mongoDocs = Configuration.TestDatabase.GetCollection<MongoDocument>("mongoDocs");
            var query = CreateQueryable<MongoDocument>(mongoDocs, ExecutionTarget.Query);

            query = query.Where(o => o.IsArchived == false);

            var model = this.GetQueryModel(query);
        }

        [Test]
        public void TestComplexArrayOrderingProjection()
        {
            var zipInfos = Configuration.TestDatabase.GetCollection<A>("zipinfos");
            var query = CreateQueryable<A>(zipInfos, ExecutionTarget.Query);

            var projection =
                query.Select(
                    o => new { Data = o.BL.OrderBy(b => b.I).Select(b => new { I = b.I }) });

            var model = this.GetQueryModel(projection);
        }

        [Test]
        public void TestPrimitiveArrayOrderingProjection()
        {
            var zipInfos = Configuration.TestDatabase.GetCollection<A>("zipinfos");
            var query = CreateQueryable<A>(zipInfos, ExecutionTarget.Query);

            var projection =
                query.Select(
                    o => new { Data = o.AI.OrderBy(c => c).Select(c => new { Value = c }) });

            var model = this.GetQueryModel(projection);
        }
    }
}