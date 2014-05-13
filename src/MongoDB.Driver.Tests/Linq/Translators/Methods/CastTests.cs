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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Cast")]
    public class CastTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The Cast query operator is not supported.")]
        public void TestCast(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c).Cast<C>();
            query.ToList(); // execute query
        }
    }
}