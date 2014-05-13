using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MongoDB.Driver.Tests.Linq
{
    [TestFixture]
    [Category("Linq")]
    public class QueryModelTests : LinqTestBase
    {
        private class A
        {
            public int I { get; set; }

            public bool B { get; set; }

            public string S { get; set; }

            public int[] AI { get; set; }
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
    }
}