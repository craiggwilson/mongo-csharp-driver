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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Min")]
    public class MinTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMinDZWithProjection(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c.D.Z).Min();
            Assert.AreEqual(11, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMinDZWithSelector(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Min(c => c.D.Z);
            Assert.AreEqual(11, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMinWithProjectionAndSelector(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c.D).Min(d => d.Z);
            Assert.AreEqual(11, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMinXWithProjection(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c.X).Min();
            Assert.AreEqual(1, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMinXWithSelector(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Min(c => c.X);
            Assert.AreEqual(1, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMinXYWithProjection(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select new { c.X, c.Y }).Min(a => a.X);
            Assert.AreEqual(1, result);
        }

        [Test]
        public void TestMinXYWithTypeProjection()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select new { c.X, c.Y }).Min();
            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }

        [Test]
        public void TestMinXYWithTypeSelector()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select c).Min(c => new { c.X, c.Y });
            Assert.AreEqual(1, result.X);
            Assert.AreEqual(11, result.Y);
        }
    }
}