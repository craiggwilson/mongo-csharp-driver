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
    [TestFixture, Category("Linq"), Category("Linq.Methods.DefaultIfEmpty")]
    public class DefaultIfEmptyTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The DefaultIfEmpty query operator is not supported.")]
        public void TestDefaultIfEmpty(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c).DefaultIfEmpty();
            query.ToList(); // execute query
        }

        [Test]
        [ExecutionTargetsTestCaseSource]
        [ExpectedException(typeof(MongoLinqException), ExpectedMessage = "The DefaultIfEmpty query operator is not supported.")]
        public void TestDefaultIfEmptyWithDefaultValue(ExecutionTarget target)
        {
            var query = (from c in CreateQueryable<C>(target)
                         select c).DefaultIfEmpty(null);
            query.ToList(); // execute query
        }
    }
}