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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Single")]
    public class SingleTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSingleOrDefaultWithManyMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).SingleOrDefault();
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleOrDefaultWithNoMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 9
                          select c).SingleOrDefault();
            Assert.IsNull(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleOrDefaultWithOneMatch(ExecutionTarget targettarget)
        {
            var result = (from c in CreateQueryable<C>()
                          where c.X == 3
                          select c).SingleOrDefault();

            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleOrDefaultWithPredicateAfterProjection(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target)
                .Select(c => c.Y)
                .SingleOrDefault(y => y == 33);

            Assert.AreEqual(33, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleOrDefaultWithPredicateAfterWhere(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 1
                          select c).SingleOrDefault(c => c.Y == 11);
            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleOrDefaultWithPredicateNoMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).SingleOrDefault(c => c.X == 9);
            Assert.IsNull(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleOrDefaultWithPredicateOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).SingleOrDefault(c => c.X == 3);
            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleOrDefaultWithPredicateTwoMatches(ExecutionTarget target)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var result = (from c in CreateQueryable<C>(target)
                              select c).SingleOrDefault(c => c.Y == 11);
            });
            Assert.AreEqual(ExpectedErrorMessage.SingleLongSequence, ex.Message);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSingleOrDefaultWithTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.Y == 11
                          select c).SingleOrDefault();
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSingleWithManyMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Single();
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSingleWithNoMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 9
                          select c).Single();
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleWithOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 3
                          select c).Single();

            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleWithPredicateAfterProjection(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target)
                .Select(c => c.Y)
                .Single(y => y == 33);

            Assert.AreEqual(33, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleWithPredicateAfterWhere(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 1
                          select c).Single(c => c.Y == 11);
            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleWithPredicateNoMatch(ExecutionTarget target)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var result = (from c in CreateQueryable<C>(target)
                              select c).Single(c => c.X == 9);
            });
            Assert.AreEqual(ExpectedErrorMessage.SingleEmptySequence, ex.Message);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleWithPredicateOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Single(c => c.X == 3);
            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestSingleWithPredicateTwoMatches(ExecutionTarget target)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var result = (from c in CreateQueryable<C>(target)
                              select c).Single(c => c.Y == 11);
            });
            Assert.AreEqual(ExpectedErrorMessage.SingleLongSequence, ex.Message);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestSingleWithTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.Y == 11
                          select c).Single();
        }
    }
}