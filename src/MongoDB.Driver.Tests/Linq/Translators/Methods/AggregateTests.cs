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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Aggregate")]
    public class AggregateTests : LinqIntegrationTestBase
    {
        [Test]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The Aggregate query operator is not supported.")]
        public void TestAggregate()
        {
            var result = (from c in CreateQueryable<C>()
                          select c).Aggregate((a, b) => null);
        }

        [Test]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The Aggregate query operator is not supported.")]
        public void TestAggregateWithAccumulatorAndSelector()
        {
            var result = (from c in CreateQueryable<C>()
                          select c).Aggregate<C, int, int>(0, (a, c) => 0, a => a);
        }
    }
}