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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Union")]
    public class UnionTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The Union query operator is not supported.")]
        public void TestUnion(ExecutionTarget target)
        {
            var source2 = new C[0];
            var query = (from c in CreateQueryable<C>(target)
                         select c).Union(source2);
            query.ToList(); // execute query
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The Union query operator is not supported.")]
        public void TestUnionWithEqualityComparer(ExecutionTarget target)
        {
            var source2 = new C[0];
            var query = (from c in CreateQueryable<C>(target)
                         select c).Union(source2, new CEqualityComparer());
            query.ToList(); // execute query
        }
    }
}