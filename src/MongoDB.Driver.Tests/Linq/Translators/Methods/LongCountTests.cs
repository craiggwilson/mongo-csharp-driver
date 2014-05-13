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
    [TestFixture, Category("Linq"), Category("Linq.Methods.LongCount")]
    public class LongCountTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLongCountEquals2(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          where c.Y == 11
                          select c).LongCount();

            Assert.AreEqual(2L, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLongCountEquals5(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).LongCount();

            Assert.AreEqual(5L, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        public void TestLongCountWithSkipAndTake(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Skip(2).Take(2).LongCount();

            Assert.AreEqual(2L, result);
        }
    }
}