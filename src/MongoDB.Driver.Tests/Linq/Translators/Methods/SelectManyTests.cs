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
    [TestFixture, Category("Linq"), Category("Linq.Methods.SelectMany")]
    public class SelectManyTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The SelectMany query operator is not supported.")]
        public void TestSelectMany(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target)
                .SelectMany(c => new C[] { c });
            query.ToList(); // execute query
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The SelectMany query operator is not supported.")]
        public void TestSelectManyWithIndex(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target)
                .SelectMany((c, index) => new C[] { c });
            query.ToList(); // execute query
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The SelectMany query operator is not supported.")]
        public void TestSelectManyWithIntermediateResults(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target)
                .SelectMany(c => new C[] { c }, (c, i) => i);
            query.ToList(); // execute query
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The SelectMany query operator is not supported.")]
        public void TestSelectManyWithIndexAndIntermediateResults(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target)
                .SelectMany((c, index) => new C[] { c }, (c, i) => i);
            query.ToList(); // execute query
        }
    }
}