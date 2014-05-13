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
    [TestFixture, Category("Linq"), Category("Linq.Methods.GroupJoin")]
    public class GroupJoinTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The GroupJoin query operator is not supported.")]
        public void TestGroupJoin(ExecutionTarget target)
        {
            var inner = new C[0];
            var query = CreateQueryable<C>(target)
                .GroupJoin(inner, c => c, c => c, (c, e) => c);
            query.ToList(); // execute query
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The GroupJoin query operator is not supported.")]
        public void TestGroupJoinWithEqualityComparer(ExecutionTarget target)
        {
            var inner = new C[0];
            var query = CreateQueryable<C>(target)
                .GroupJoin(inner, c => c, c => c, (c, e) => c, new CEqualityComparer());
            query.ToList(); // execute query
        }
    }
}