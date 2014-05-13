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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Join")]
    public class JoinTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The Join query operator is not supported.")]
        public void TestJoin(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target)
                .Join(CreateQueryable<C>(), c => c.X, c => c.X, (x, y) => x);
            query.ToList(); // execute query
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The Join query operator is not supported.")]
        public void TestJoinWithEqualityComparer(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target)
                .Join(CreateQueryable<C>(), c => c.X, c => c.X, (x, y) => x, new Int32EqualityComparer());
            query.ToList(); // execute query
        }
    }
}