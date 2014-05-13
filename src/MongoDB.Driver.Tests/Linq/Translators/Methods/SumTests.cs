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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Sum")]
    public class SumTests : LinqIntegrationTestBase
    {
        [Test]
        [ExpectedException(typeof(MongoLinqException))]
        public void TestSumWithQuery()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Query)
                          select c.D.Z).Sum();
        }

        [Test]
        public void TestSumDZWithProjection()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select c.D.Z).Sum();
            Assert.AreEqual(165, result);
        }

        [Test]
        public void TestSumDZWithSelector()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select c).Sum(c => c.D.Z);
            Assert.AreEqual(165, result);
        }

        [Test]
        public void TestSumWithProjectionAndSelector()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select c.D).Sum(d => d.Z);
            Assert.AreEqual(165, result);
        }

        [Test]
        public void TestSumXWithProjection()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select c.X).Sum();
            Assert.AreEqual(15, result);
        }

        [Test]
        public void TestSumXWithSelector()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select c).Sum(c => c.X);
            Assert.AreEqual(15, result);
        }

        [Test]
        public void TestSumXYWithProjection()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select new { c.X, c.Y }).Sum(a => a.X);
            Assert.AreEqual(15, result);
        }
    }
}