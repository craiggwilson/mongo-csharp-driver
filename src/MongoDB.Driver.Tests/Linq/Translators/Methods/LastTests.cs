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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Last")]
    public class LastTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastOrDefaultWithManyMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).LastOrDefault();

            Assert.AreEqual(4, result.X);
            Assert.AreEqual(44, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastOrDefaultWithNoMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 9
                          select c).LastOrDefault();
            Assert.IsNull(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastOrDefaultWithOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 3
                          select c).LastOrDefault();

            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastOrDefaultWithOrderBy(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          orderby c.X
                          select c).LastOrDefault();

            Assert.AreEqual(5, result.X);
            Assert.AreEqual(44, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastOrDefaultWithPredicateAfterProjection(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target)
                .Select(c => c.Y)
                .LastOrDefault(y => y == 11);

            Assert.AreEqual(11, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastOrDefaultWithPredicateAfterWhere(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 1
                          select c).LastOrDefault(c => c.Y == 11);
            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastOrDefaultWithPredicateNoMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).LastOrDefault(c => c.X == 9);
            Assert.IsNull(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastOrDefaultWithPredicateOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).LastOrDefault(c => c.X == 3);
            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastOrDefaultWithPredicateTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).LastOrDefault(c => c.Y == 11);
            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastOrDefaultWithTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.Y == 11
                          select c).LastOrDefault();

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastWithManyMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Last();

            Assert.AreEqual(4, result.X);
            Assert.AreEqual(44, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestLastWithNoMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 9
                          select c).Last();
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastWithOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 3
                          select c).Last();

            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastWithPredicateAfterProjection(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target)
                .Select(c => c.Y)
                .Last(y => y == 11);

            Assert.AreEqual(11, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastWithPredicateAfterWhere(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 1
                          select c).Last(c => c.Y == 11);
            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastWithPredicateNoMatch(ExecutionTarget target)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var result = (from c in CreateQueryable<C>(target)
                              select c).Last(c => c.X == 9);
            });
            Assert.AreEqual(ExpectedErrorMessage.LastEmptySequence, ex.Message);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastWithPredicateOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Last(c => c.X == 3);
            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastWithPredicateTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Last(c => c.Y == 11);
            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastWithOrderBy(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          orderby c.X
                          select c).Last();

            Assert.AreEqual(5, result.X);
            Assert.AreEqual(44, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLastWithTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.Y == 11
                          select c).Last();

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }
    }
}