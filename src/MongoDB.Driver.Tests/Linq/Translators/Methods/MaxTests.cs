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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Max")]
    public class MaxTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMaxDZWithProjection(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c.D.Z).Max();
            Assert.AreEqual(55, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMaxDZWithSelector(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Max(c => c.D.Z);
            Assert.AreEqual(55, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMaxWithProjectionAndSelector(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c.D).Max(d => d.Z);
            Assert.AreEqual(55, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMaxXWithProjection(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c.X).Max();
            Assert.AreEqual(5, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMaxXWithSelector(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select c).Max(c => c.X);
            Assert.AreEqual(5, result);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestMaxXYWithProjection(ExecutionTarget target)
        {
            var result = (from c in CreateQueryable<C>(target)
                          select new { c.X, c.Y }).Max(a => a.X);
            Assert.AreEqual(5, result);
        }

        [Test]
        public void TestMaxXYWithTypeProjection()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select new { c.X, c.Y }).Max();
            Assert.AreEqual(5, result.X);
            Assert.AreEqual(44, result.Y);
        }

        [Test]
        public void TestMaxXYWithTypeSelector()
        {
            var result = (from c in CreateQueryable<C>(ExecutionTarget.Pipeline)
                          select c).Max(c => new { c.X, c.Y });
            Assert.AreEqual(5, result.X);
            Assert.AreEqual(44, result.Y);
        }
    }
}