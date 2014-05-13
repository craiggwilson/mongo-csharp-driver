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
    [TestFixture, Category("Linq"), Category("Linq.Methods.ElementAt")]
    public class ElementAtTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestElementAtOrDefaultWithManyMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).ElementAtOrDefault(2);

            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestElementAtOrDefaultWithNoMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 9
                          select c).ElementAtOrDefault(0);
            Assert.IsNull(result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestElementAtOrDefaultWithOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 3
                          select c).ElementAtOrDefault(0);

            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestElementAtOrDefaultWithTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.Y == 11
                          select c).ElementAtOrDefault(1);

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestElementAtWithManyMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).ElementAt(2);

            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [ExecutionTargetsTestCaseSource]
        public void TestElementAtWithNoMatch(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         where c.X == 9
                         select c).ElementAt(0);
            }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestElementAtWithOneMatch(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.X == 3
                          select c).ElementAt(0);

            Assert.AreEqual(3, result.X);
            Assert.AreEqual(33, result.Y);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestElementAtWithTwoMatches(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.Y == 11
                          select c).ElementAt(1);

            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }
    }
}