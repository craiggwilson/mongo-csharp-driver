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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Any")]
    public class AnyTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestAny(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Any();

            Assert.IsTrue(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestAnyWhereXEquals1(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 1
                          select c).Any();

            Assert.IsTrue(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestAnyWhereXEquals9(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 9
                          select c).Any();

            Assert.IsFalse(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestAnyWhereXEquals9Skip1(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 9
                          select c).Skip(1).Any();

            Assert.IsFalse(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestAnyWithPredicateAfterProjection(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target)
                .Select(c => c.D.Z)
                .Any(y => y == 11);

            Assert.IsTrue(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestAnyWithPredicateAfterWhere(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target)
                .Where(c => c.X == 1)
                .Any(c => c.Y == 11);

            Assert.IsTrue(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestAnyWithPredicateFalse(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target).Any(c => c.X == 9);

            Assert.IsFalse(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestAnyWithPredicateTrue(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target).Any(c => c.X == 1);

            Assert.IsTrue(result);
        }
    }
}