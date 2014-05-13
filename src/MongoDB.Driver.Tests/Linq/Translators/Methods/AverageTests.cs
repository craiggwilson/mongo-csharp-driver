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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Average")]
    public class AverageTests : LinqIntegrationTestBase
    {
        [Test]
        [ExpectedException(typeof(MongoLinqException))]
        public void TestAverageWithQuery()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Query)
                         select c.X).Average();
        }

        [Test]
        public void TestAverage()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select c.X).Average();

            Assert.AreEqual(3, result);
        }

        [Test]
        public void TestAverageNullable()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select c.NullableDouble).Average();

            Assert.AreEqual(3, result);
        }

        [Test]
        public void TestAverageWithSelector()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select c).Average(c => c.X);

            Assert.AreEqual(3, result);
        }

        [Test]
        public void TestAverageWithSelectorNullable()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select c).Average(c => c.NullableDouble);

            Assert.AreEqual(3, result);
        }
    }
}