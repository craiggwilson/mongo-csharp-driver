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
    [TestFixture, Category("Linq"), Category("Linq.Methods.SequenceEqual")]
    public class SequenceEqualTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The SequenceEqual query operator is not supported.")]
        public void TestSequenceEqual(ExecutionTarget target)
        {
            var source2 = new C[0];
            var result = (from c in CreateQueryable<C>(target)
                          select c).SequenceEqual(source2);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The SequenceEqual query operator is not supported.")]
        public void TestSequenceEqualtWithEqualityComparer(ExecutionTarget target)
        {
            var source2 = new C[0];
            var result = (from c in CreateQueryable<C>(target)
                          select c).SequenceEqual(source2, new CEqualityComparer());
        }
    }
}