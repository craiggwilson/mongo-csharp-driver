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
    [TestFixture, Category("Linq"), Category("Linq.Methods.First")]
    public class FirstTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstOrDefaultWithManyMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).FirstOrDefault();

            Assert.AreEqual(2, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstOrDefaultWithNoMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 9
                          select c).FirstOrDefault();
            Assert.IsNull(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstOrDefaultWithNoMatchAndProjectionToStruct(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 9
                          select c.X).FirstOrDefault();
            Assert.AreEqual(0, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstOrDefaultWithOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 3
                          select c).FirstOrDefault();

            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstOrDefaultWithPredicateAfterProjection(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target)
                .Select(c => c.Y)
                .FirstOrDefault(y => y == 11);

            Assert.AreEqual(11, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstOrDefaultWithPredicateAfterWhere(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 1
                          select c).FirstOrDefault(c => c.Y == 11);
            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstOrDefaultWithPredicateNoMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).FirstOrDefault(c => c.X == 9);
            Assert.IsNull(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstOrDefaultWithPredicateOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).FirstOrDefault(c => c.X == 3);
            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstOrDefaultWithPredicateTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).FirstOrDefault(c => c.Y == 11);
            Assert.AreEqual(2, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstOrDefaultWithTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.Y == 11
                          select c).FirstOrDefault();

            Assert.AreEqual(2, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstWithManyMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).First();

            Assert.AreEqual(2, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstWithNoMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 9
                          select c).First();
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstWithOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 3
                          select c).First();

            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstWithPredicateAfterProjection(ExecutionTarget target)
        {
            var result = CreateQueryable<C>(target)
                .Select(c => c.Y)
                .First(y => y == 11);

            Assert.AreEqual(11, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstWithPredicateAfterWhere(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 1
                          select c).First(c => c.Y == 11);
            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstWithPredicateNoMatch(ExecutionTarget target)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                var result = (from c in CreateQueryable<C>(target)
                              select c).First(c => c.X == 9);
            });
            Assert.AreEqual(ExpectedErrorMessage.FirstEmptySequence, ex.Message);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstWithPredicateOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).First(c => c.X == 3);
            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstWithPredicateTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).First(c => c.Y == 11);
            Assert.AreEqual(2, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestFirstWithTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.Y == 11
                          select c).First();

            Assert.AreEqual(2, result.X);
            Assert.AreEqual(11, result.Y);
        }
    }
}