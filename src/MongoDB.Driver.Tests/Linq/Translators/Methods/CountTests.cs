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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Count")]
    public class CountTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestCountEquals2(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.Y == 11
                          select c).Count();

            Assert.AreEqual(2, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestCountEquals5(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Count();

            Assert.AreEqual(5, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestCountWithPredicate(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target).Count(c => c.Y == 11);

            Assert.AreEqual(2, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestCountWithPredicateAfterProjection(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target)
                .Select(c => c.Y)
                .Count(y => y == 11);

            Assert.AreEqual(2, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestCountWithPredicateAfterWhere(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target)
                .Where(c => c.X == 1)
                .Count(c => c.Y == 11);

            Assert.AreEqual(1, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestCountWithSkipAndTake(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target)
                .Skip(2)
                .Take(2)
                .Count();

            Assert.AreEqual(2, result);
        }
    }
}