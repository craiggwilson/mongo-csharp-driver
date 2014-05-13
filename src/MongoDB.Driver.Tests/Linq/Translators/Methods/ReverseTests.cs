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
    [TestFixture, Category("Linq"), Category("Linq.Methods.Reverse")]
    public class ReverseTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The Reverse query operator is not supported.")]
        public void TestReverse(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c).Reverse();
            query.ToList(); // execute query
        }
    }
}