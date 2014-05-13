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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Contains")]
    public class ContainsTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The Contains query operator is not supported.")]
        public void TestContains(ExecutionTarget target)
        {
            var item = new C();
            var result = (from c in CreateQueryable<C>(target)
                          select c).Contains(item);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The Contains query operator is not supported.")]
        public void TestContainsWithEqualityComparer(ExecutionTarget target)
        {
            var item = new C();
            var result = (from c in CreateQueryable<C>(target)
                          select c).Contains(item, new CEqualityComparer());
        }
    }
}