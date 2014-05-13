using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MongoDB.Driver.Builders;

namespace MongoDB.Driver.Tests.Linq.Translators.Methods
{
    [TestFixture, Category("Linq"), Category("Linq.Methods.Where")]
    public class WhereTests : LinqIntegrationTestBase
    {
        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereAAny(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.A.Any()
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a\" : { \"$ne\" : null, \"$not\" : { \"$size\" : 0 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereAAnyWithPredicate(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.A.Any(a => a > 3)
                        select c;

            Assert.Throws<MongoLinqException>(() => query.ToList());
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLocalIListContainsX(ExecutionTarget target)
        {
            IList<int> local = new [] { 1, 2, 3 };

            var query = from c in CreateQueryable<C>(target)
                        where local.Contains(c.X)
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"x\" : { \"$in\" : [1, 2, 3] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLocalListContainsX(ExecutionTarget target)
        {
            var local = new List<int> { 1, 2, 3 };

            var query = from c in CreateQueryable<C>(target)
                        where local.Contains(c.X)
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"x\" : { \"$in\" : [1, 2, 3] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLocalArrayContainsX(ExecutionTarget target)
        {
            var local = new[] { 1, 2, 3 };

            var query = from c in CreateQueryable<C>(target)
                        where local.Contains(c.X)
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"x\" : { \"$in\" : [1, 2, 3] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereAContains2(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.A.Contains(2)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a\" : 2 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereAContains2Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.A.Contains(2)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"a\" : { \"$ne\" : 2 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereAContainsAll(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.A.ContainsAll(new[] { 2, 3 })
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a\" : { \"$all\" : [2, 3] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereAContainsAllNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.A.ContainsAll(new[] { 2, 3 })
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"a\" : { \"$not\" : { \"$all\" : [2, 3] } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereAContainsAny(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.A.ContainsAny(new[] { 2, 3 })
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a\" : { \"$in\" : [2, 3] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereAContainsAnyNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.A.ContainsAny(new[] { 1, 2 })
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"a\" : { \"$nin\" : [1, 2] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereAExistsFalse(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where Query.NotExists("a").Inject()
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"a\" : { \"$exists\" : false } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereAExistsTrue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where Query.Exists("a").Inject()
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a\" : { \"$exists\" : true } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereAExistsTrueNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !Query.Exists("a").Inject()
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"a\" : { \"$exists\" : false } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereALengthEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.A.Length == 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a\" : { \"$size\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereALengthEquals3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.A.Length == 3)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"a\" : { \"$not\" : { \"$size\" : 3 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereALengthEquals3Reversed(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where 3 == c.A.Length
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a\" : { \"$size\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereALengthNotEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.A.Length != 3
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"a\" : { \"$not\" : { \"$size\" : 3 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereALengthNotEquals3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.A.Length != 3)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a\" : { \"$size\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereASub1Equals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.A[1] == 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a.1\" : 3 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereASub1Equals3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.A[1] == 3)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"a.1\" : { \"$ne\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereASub1ModTwoEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.A[1] % 2 == 1
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a.1\" : { \"$mod\" : [2, 1] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereASub1ModTwoEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.A[1] % 2 == 1)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"a.1\" : { \"$not\" : { \"$mod\" : [2, 1] } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereASub1ModTwoNotEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.A[1] % 2 != 1
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"a.1\" : { \"$not\" : { \"$mod\" : [2, 1] } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereASub1ModTwoNotEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.A[1] % 2 != 1)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a.1\" : { \"$mod\" : [2, 1] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereASub1NotEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.A[1] != 3
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"a.1\" : { \"$ne\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereASub1NotEquals3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.A[1] != 3)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"a.1\" : 3 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereB(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.B
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"b\" : true }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereBASub0(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.BA[0]
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"ba.0\" : true }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereBASub0EqualsFalse(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.BA[0] == false
                        select c;

            AssertQueryOrPipeline(query, 0, "{ \"ba.0\" : false }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereBASub0EqualsFalseNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.BA[0] == false)
                        select c;

            AssertQueryOrPipeline(query, 5, "{ \"ba.0\" : { \"$ne\" : false } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereBASub0EqualsTrue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.BA[0] == true
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"ba.0\" : true }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereBASub0EqualsTrueNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.BA[0] == true)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"ba.0\" : { \"$ne\" : true } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereBASub0Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.BA[0]
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"ba.0\" : { \"$ne\" : true } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereBEqualsFalse(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.B == false
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"b\" : false }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereBEqualsFalseNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.B == false)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"b\" : { \"$ne\" : false } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereBEqualsTrue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.B == true
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"b\" : true }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereBEqualsTrueNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.B == true)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"b\" : { \"$ne\" : true } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereBNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.B
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"b\" : { \"$ne\" : true } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDBRefCollectionNameEqualsC(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.DBRef.CollectionName == "c"
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"dbref.$ref\" : \"c\" }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDBRefDatabaseNameEqualsDb(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.DBRef.DatabaseName == "db"
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"dbref.$db\" : \"db\" }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDBRefEquals(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.DBRef == new MongoDBRef("db", "c", 1)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"dbref\" : { \"$ref\" : \"c\", \"$id\" : 1, \"$db\" : \"db\" } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDBRefEqualsNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.DBRef == new MongoDBRef("db", "c", 1))
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"dbref\" : { \"$ne\" : { \"$ref\" : \"c\", \"$id\" : 1, \"$db\" : \"db\" } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDBRefNotEquals(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.DBRef != new MongoDBRef("db", "c", 1)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"dbref\" : { \"$ne\" : { \"$ref\" : \"c\", \"$id\" : 1, \"$db\" : \"db\" } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDBRefNotEqualsNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.DBRef != new MongoDBRef("db", "c", 1))
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"dbref\" : { \"$ref\" : \"c\", \"$id\" : 1, \"$db\" : \"db\" } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDBRefIdEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.DBRef.Id == 1
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"dbref.$id\" : 1 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDEquals11(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.D == new D { Z = 11 }
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"d\" : { \"z\" : 11 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDEquals11Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.D == new D { Z = 11 })
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"d\" : { \"$ne\" : { \"z\" : 11 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDNotEquals11(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.D != new D { Z = 11 }
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"d\" : { \"$ne\" : { \"z\" : 11 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDNotEquals11Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.D != new D { Z = 11 })
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"d\" : { \"z\" : 11 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereDAAnyWithPredicate(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.DA.Any(d => d.Z == 333)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"da\" : { \"$elemMatch\" : { \"z\" : 333 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEAContainsAll(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.EA.ContainsAll(new E[] { E.A, E.B })
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"ea\" : { \"$all\" : [1, 2] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEAContainsAllNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.EA.ContainsAll(new E[] { E.A, E.B })
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"ea\" : { \"$not\" : { \"$all\" : [1, 2] } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEAContainsAny(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.EA.ContainsAny(new[] { E.A, E.B })
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"ea\" : { \"$in\" : [1, 2] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEAContainsAnyNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.EA.ContainsAny(new[] { E.A, E.B })
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"ea\" : { \"$nin\" : [1, 2] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEAContainsB(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.EA.Contains(E.B)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"ea\" : 2 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEAContainsBNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.EA.Contains(E.B)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"ea\" : { \"$ne\" : 2 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEASub0EqualsA(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.EA[0] == E.A
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"ea.0\" : 1 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEASub0EqualsANot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.EA[0] == E.A)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"ea.0\" : { \"$ne\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEASub0NotEqualsA(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.EA[0] != E.A
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"ea.0\" : { \"$ne\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEASub0NotEqualsANot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.EA[0] != E.A)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"ea.0\" : 1 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEEqualsA(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.E == E.A
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"e\" : \"A\" }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEEqualsANot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.E == E.A)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"e\" : { \"$ne\" : \"A\" } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEEqualsAReversed(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where E.A == c.E
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"e\" : \"A\" }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEInAOrB(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.E.In(new[] { E.A, E.B })
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"e\" : { \"$in\" : [\"A\", \"B\"] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereEInAOrBNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.E.In(new[] { E.A, E.B })
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"e\" : { \"$nin\" : [\"A\", \"B\"] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereENotEqualsA(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.E != E.A
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"e\" : { \"$ne\" : \"A\" } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereENotEqualsANot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.E != E.A)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"e\" : \"A\" }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLContains2(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.L.Contains(2)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l\" : 2 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLContains2Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.L.Contains(2)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l\" : { \"$ne\" : 2 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLContainsAll(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.L.ContainsAll(new[] { 2, 3 })
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l\" : { \"$all\" : [2, 3] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLContainsAllNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.L.ContainsAll(new[] { 2, 3 })
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l\" : { \"$not\" : { \"$all\" : [2, 3] } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLContainsAny(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.L.ContainsAny(new[] { 2, 3 })
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l\" : { \"$in\" : [2, 3] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLContainsAnyNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.L.ContainsAny(new[] { 1, 2 })
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l\" : { \"$nin\" : [1, 2] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLExistsFalse(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where Query.NotExists("l").Inject()
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l\" : { \"$exists\" : false } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLExistsTrue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where Query.Exists("l").Inject()
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l\" : { \"$exists\" : true } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLExistsTrueNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !Query.Exists("l").Inject()
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l\" : { \"$exists\" : false } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLCountMethodEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.L.Count() == 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l\" : { \"$size\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLCountMethodEquals3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.L.Count() == 3)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l\" : { \"$not\" : { \"$size\" : 3 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLCountMethodEquals3Reversed(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where 3 == c.L.Count()
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l\" : { \"$size\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLCountPropertyEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.L.Count == 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l\" : { \"$size\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLCountPropertyEquals3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.L.Count == 3)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l\" : { \"$not\" : { \"$size\" : 3 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLCountPropertyEquals3Reversed(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where 3 == c.L.Count
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l\" : { \"$size\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLCountPropertyNotEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.L.Count != 3
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l\" : { \"$not\" : { \"$size\" : 3 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLCountPropertyNotEquals3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.L.Count != 3)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l\" : { \"$size\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLSub1Equals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.L[1] == 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l.1\" : 3 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLSub1Equals3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.L[1] == 3)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l.1\" : { \"$ne\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLSub1ModTwoEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.L[1] % 2 == 1
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l.1\" : { \"$mod\" : [2, 1] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLSub1ModTwoEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.L[1] % 2 == 1)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l.1\" : { \"$not\" : { \"$mod\" : [2, 1] } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLSub1ModTwoNotEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.L[1] % 2 != 1
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l.1\" : { \"$not\" : { \"$mod\" : [2, 1] } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLSub1ModTwoNotEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.L[1] % 2 != 1)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l.1\" : { \"$mod\" : [2, 1] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLSub1NotEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.L[1] != 3
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"l.1\" : { \"$ne\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLSub1NotEquals3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.L[1] != 3)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"l.1\" : 3 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLXModTwoEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.LX % 2 == 1
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"lx\" : { \"$mod\" : [2, 1] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLXModTwoEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.LX % 2 == 1)
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"lx\" : { \"$not\" : { \"$mod\" : [2, 1] } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLXModTwoEquals1Reversed(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where 1 == c.LX % 2
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"lx\" : { \"$mod\" : [2, 1] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLXModTwoNotEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.LX % 2 != 1
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"lx\" : { \"$not\" : { \"$mod\" : [2, 1] } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereLXModTwoNotEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.LX % 2 != 1)
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"lx\" : { \"$mod\" : [2, 1] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSASub0ContainsO(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where c.SA[0].Contains("o")
                            select c;

                AssertQueryOrPipeline(query, 1, "{ \"sa.0\" : /o/s }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSASub0ContainsONot(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where !c.SA[0].Contains("o")
                            select c;

                AssertQueryOrPipeline(query, 4, "{ \"sa.0\" : { \"$not\" : /o/s } }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSASub0EndsWithM(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where c.SA[0].EndsWith("m")
                            select c;

                AssertQueryOrPipeline(query, 1, "{ \"sa.0\" : /m$/s }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSASub0EndsWithMNot(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where !c.SA[0].EndsWith("m")
                            select c;

                AssertQueryOrPipeline(query, 4, "{ \"sa.0\" : { \"$not\" : /m$/s } }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSASub0IsMatch(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var regex = new Regex(@"^T");
                var query = from c in CreateQueryable<C>(target)
                            where regex.IsMatch(c.SA[0])
                            select c;

                AssertQueryOrPipeline(query, 1, "{ \"sa.0\" : /^T/ }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSASub0IsMatchNot(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var regex = new Regex(@"^T");
                var query = from c in CreateQueryable<C>(target)
                            where !regex.IsMatch(c.SA[0])
                            select c;

                AssertQueryOrPipeline(query, 4, "{ \"sa.0\" : { \"$not\" : /^T/ } }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSASub0IsMatchStatic(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where Regex.IsMatch(c.SA[0], "^T")
                            select c;

                AssertQueryOrPipeline(query, 1, "{ \"sa.0\" : /^T/ }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSASub0IsMatchStaticNot(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where !Regex.IsMatch(c.SA[0], "^T")
                            select c;

                AssertQueryOrPipeline(query, 4, "{ \"sa.0\" : { \"$not\" : /^T/ } }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSASub0IsMatchStaticWithOptions(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where Regex.IsMatch(c.SA[0], "^t", RegexOptions.IgnoreCase)
                            select c;

                AssertQueryOrPipeline(query, 1, "{ \"sa.0\" : /^t/i }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSASub0StartsWithT(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where c.SA[0].StartsWith("T")
                            select c;

                AssertQueryOrPipeline(query, 1, "{ \"sa.0\" : /^T/s }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSASub0StartsWithTNot(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(1, 8, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where !c.SA[0].StartsWith("T")
                            select c;

                AssertQueryOrPipeline(query, 4, "{ \"sa.0\" : { \"$not\" : /^T/s } }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSContainsAbc(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Contains("abc")
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /abc/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSContainsAbcNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.S.Contains("abc")
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /abc/s } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSContainsDot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Contains(".")
                        select c;

            AssertQueryOrPipeline(query, 0, "{ \"s\" : /\\./s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSCountEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Count() == 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{3}$/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSEqualsAbc(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S == "abc"
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : \"abc\" }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSEqualsAbcNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.S == "abc")
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$ne\" : \"abc\" } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSEqualsMethodAbc(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Equals("abc")
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : \"abc\" }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSEqualsMethodAbcNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.S.Equals("abc"))
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$ne\" : \"abc\" } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSEqualsStaticMethodAbc(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where string.Equals(c.S, "abc")
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : \"abc\" }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSEqualsStaticMethodAbcNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !string.Equals(c.S, "abc")
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$ne\" : \"abc\" } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSEndsWithAbc(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.EndsWith("abc")
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /abc$/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSEndsWithAbcNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.S.EndsWith("abc")
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /abc$/s } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfAnyBC(ExecutionTarget target)
        {
            var tempCollection = _database.GetCollection("temp");
            tempCollection.Drop();
            tempCollection.Insert(new C { S = "bxxx" });
            tempCollection.Insert(new C { S = "xbxx" });
            tempCollection.Insert(new C { S = "xxbx" });
            tempCollection.Insert(new C { S = "xxxb" });
            tempCollection.Insert(new C { S = "bxbx" });
            tempCollection.Insert(new C { S = "xbbx" });
            tempCollection.Insert(new C { S = "xxbb" });

            var query1 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOfAny(new char[] { 'b', 'c' }) == 2
                select c;
            Assert.AreEqual(2, query1.ToList().Count);

            var query2 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOfAny(new char[] { 'b', 'c' }, 1) == 2
                select c;
            Assert.AreEqual(3, query2.ToList().Count);

            var query3 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOfAny(new char[] { 'b', 'c' }, 1, 1) == 2
                select c;
            Assert.AreEqual(0, query3.ToList().Count);

            var query4 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOfAny(new char[] { 'b', 'c' }, 1, 2) == 2
                select c;
            Assert.AreEqual(3, query4.ToList().Count);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfAnyBDashCEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.IndexOfAny(new char[] { 'b', '-', 'c' }) == 1
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^[^b\\-c]{1}[b\\-c]/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfAnyBCStartIndex1Equals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.IndexOfAny(new char[] { 'b', '-', 'c' }, 1) == 1
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{1}[^b\\-c]{0}[b\\-c]/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfAnyBCStartIndex1Count2Equals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.IndexOfAny(new char[] { 'b', '-', 'c' }, 1, 2) == 1
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{1}(?=.{2})[^b\\-c]{0}[b\\-c]/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfB(ExecutionTarget target)
        {
            var tempCollection = _database.GetCollection("temp");
            tempCollection.Drop();
            tempCollection.Insert(new C { S = "bxxx" });
            tempCollection.Insert(new C { S = "xbxx" });
            tempCollection.Insert(new C { S = "xxbx" });
            tempCollection.Insert(new C { S = "xxxb" });
            tempCollection.Insert(new C { S = "bxbx" });
            tempCollection.Insert(new C { S = "xbbx" });
            tempCollection.Insert(new C { S = "xxbb" });

            var query1 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOf('b') == 2
                select c;
            Assert.AreEqual(2, query1.ToList().Count);

            var query2 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOf('b', 1) == 2
                select c;
            Assert.AreEqual(3, query2.ToList().Count);

            var query3 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOf('b', 1, 1) == 2
                select c;
            Assert.AreEqual(0, query3.ToList().Count);

            var query4 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOf('b', 1, 2) == 2
                select c;
            Assert.AreEqual(3, query4.ToList().Count);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfBEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.IndexOf('b') == 1
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^[^b]{1}b/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfBStartIndex1Equals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.IndexOf('b', 1) == 1
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{1}[^b]{0}b/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfBStartIndex1Count2Equals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.IndexOf('b', 1, 2) == 1
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{1}(?=.{2})[^b]{0}b/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfXyz(ExecutionTarget target)
        {
            var tempCollection = _database.GetCollection("temp");
            tempCollection.Drop();
            tempCollection.Insert(new C { S = "xyzaaa" });
            tempCollection.Insert(new C { S = "axyzaa" });
            tempCollection.Insert(new C { S = "aaxyza" });
            tempCollection.Insert(new C { S = "aaaxyz" });
            tempCollection.Insert(new C { S = "aaaaxy" });
            tempCollection.Insert(new C { S = "xyzxyz" });

            var query1 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOf("xyz") == 3
                select c;
            Assert.AreEqual(1, query1.ToList().Count);

            var query2 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOf("xyz", 1) == 3
                select c;
            Assert.AreEqual(2, query2.ToList().Count);

            var query3 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOf("xyz", 1, 4) == 3
                select c;
            Assert.AreEqual(0, query3.ToList().Count); // substring isn't long enough to match

            var query4 =
                from c in CreateQueryable<C>(tempCollection)
                where c.S.IndexOf("xyz", 1, 5) == 3
                select c;
            Assert.AreEqual(2, query4.ToList().Count);
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfXyzEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.IndexOf("xyz") == 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^(?!.{0,2}xyz).{3}xyz/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfXyzStartIndex1Equals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.IndexOf("xyz", 1) == 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{1}(?!.{0,1}xyz).{2}xyz/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIndexOfXyzStartIndex1Count5Equals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.IndexOf("xyz", 1, 5) == 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{1}(?=.{5})(?!.{0,1}xyz).{2}xyz/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIsMatch(ExecutionTarget target)
        {
            var regex = new Regex(@"^abc");
            var query = from c in CreateQueryable<C>(target)
                        where regex.IsMatch(c.S)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^abc/ }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIsMatchNot(ExecutionTarget target)
        {
            var regex = new Regex(@"^abc");
            var query = from c in CreateQueryable<C>(target)
                        where !regex.IsMatch(c.S)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /^abc/ } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIsMatchStatic(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where Regex.IsMatch(c.S, "^abc")
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^abc/ }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIsMatchStaticNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !Regex.IsMatch(c.S, "^abc")
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /^abc/ } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIsMatchStaticWithOptions(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where Regex.IsMatch(c.S, "^abc", RegexOptions.IgnoreCase)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^abc/i }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSIsNullOrEmpty(ExecutionTarget target)
        {
            var tempCollection = _database.GetCollection("temp");
            tempCollection.Drop();
            tempCollection.Insert(new C()); // serialized document will have no "s" field
            tempCollection.Insert(new BsonDocument("s", BsonNull.Value)); // work around [BsonIgnoreIfNull] on S
            tempCollection.Insert(new C { S = "" });
            tempCollection.Insert(new C { S = "x" });

            var query = from c in CreateQueryable<C>(tempCollection)
                        where string.IsNullOrEmpty(c.S)
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"$or\" : [{ \"s\" : { \"$type\" : 10 } }, { \"s\" : \"\" }] }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSLengthEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Length == 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{3}$/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSLengthEquals3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.S.Length == 3)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /^.{3}$/s } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSLengthGreaterThan3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Length > 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{4,}$/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSLengthGreaterThanOrEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Length >= 3
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"s\" : /^.{3,}$/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSLengthLessThan3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Length < 3
                        select c;

            AssertQueryOrPipeline(query, 0, "{ \"s\" : /^.{0,2}$/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSLengthLessThanOrEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Length <= 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{0,3}$/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSLengthNotEquals3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Length != 3
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /^.{3}$/s } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSLengthNotEquals3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.S.Length != 3)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{3}$/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSNotEqualsAbc(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S != "abc"
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$ne\" : \"abc\" } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSNotEqualsAbcNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.S != "abc")
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : \"abc\" }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSStartsWithAbc(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.StartsWith("abc")
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^abc/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSStartsWithAbcNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.S.StartsWith("abc")
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /^abc/s } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSSub1EqualsB(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S[1] == 'b'
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{1}b/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSSub1EqualsBNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.S[1] == 'b')
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /^.{1}b/s } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSSub1NotEqualsB(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S[1] != 'b'
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^.{1}[^b]/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSSub1NotEqualsBNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.S[1] != 'b')
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /^.{1}[^b]/s } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSTrimContainsXyz(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Trim().Contains("xyz")
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^\\s*.*xyz.*\\s*$/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSTrimContainsXyzNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.S.Trim().Contains("xyz")
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /^\\s*.*xyz.*\\s*$/s } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSTrimEndsWithXyz(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Trim().EndsWith("xyz")
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^\\s*.*xyz\\s*$/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSTrimEndsWithXyzNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.S.Trim().EndsWith("xyz")
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /^\\s*.*xyz\\s*$/s } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSTrimStartsWithXyz(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.Trim().StartsWith("xyz")
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^\\s*xyz.*\\s*$/s }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSTrimStartsWithXyzNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.S.Trim().StartsWith("xyz")
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /^\\s*xyz.*\\s*$/s } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSTrimStartTrimEndToLowerContainsXyz(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.TrimStart(' ', '.', '-', '\t').TrimEnd().ToLower().Contains("xyz")
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^[\\ \\.\\-\\t]*.*xyz.*\\s*$/is }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToLowerEqualsConstantLowerCaseValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToLower() == "abc"
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"s\" : /^abc$/i }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToLowerDoesNotEqualConstantLowerCaseValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToLower() != "abc"
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"s\" : { \"$not\" : /^abc$/i } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToLowerEqualsConstantMixedCaseValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToLower() == "Abc"
                        select c;

            AssertQueryOrPipeline(query, 0, "{ \"_id\" : { \"$type\" : -1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToLowerDoesNotEqualConstantMixedCaseValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToLower() != "Abc"
                        select c;

            AssertQueryOrPipeline(query, 5, "{ }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToLowerEqualsNullValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToLower() == null
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"s\" : null }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToLowerDoesNotEqualNullValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToLower() != null
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"s\" : { \"$ne\" : null } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToUpperEqualsConstantLowerCaseValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToUpper() == "abc"
                        select c;

            AssertQueryOrPipeline(query, 0, "{ \"_id\" : { \"$type\" : -1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToUpperDoesNotEqualConstantLowerCaseValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToUpper() != "abc"
                        select c;

            AssertQueryOrPipeline(query, 5, "{ }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToUpperEqualsConstantMixedCaseValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToUpper() == "Abc"
                        select c;

            AssertQueryOrPipeline(query, 0, "{ \"_id\" : { \"$type\" : -1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToUpperDoesNotEqualConstantMixedCaseValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToUpper() != "Abc"
                        select c;

            AssertQueryOrPipeline(query, 5, "{ }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToUpperEqualsNullValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToUpper() == null
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"s\" : null }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSToUpperDoesNotEqualNullValue(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.S.ToUpper() != null
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"s\" : { \"$ne\" : null } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSystemProfileInfoDurationGreatherThan10Seconds(ExecutionTarget target)
        {
            var query = from pi in CreateQueryable<SystemProfileInfo>(_systemProfileCollection)
                        where pi.Duration > TimeSpan.FromSeconds(10)
                        select pi;

            var model = GetQueryModel(query);

            Assert.AreEqual("{ \"millis\" : { \"$gt\" : 10000.0 } }", model.Query.ToJson());
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSystemProfileInfoNamespaceEqualsNs(ExecutionTarget target)
        {
            var query = from pi in CreateQueryable<SystemProfileInfo>(_systemProfileCollection)
                        where pi.Namespace == "ns"
                        select pi;

            var model = GetQueryModel(query);

            Assert.AreEqual("{ \"ns\" : \"ns\" }", model.Query.ToJson());
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSystemProfileInfoNumberScannedGreaterThan1000(ExecutionTarget target)
        {
            var query = from pi in CreateQueryable<SystemProfileInfo>(_systemProfileCollection)
                        where pi.NumberScanned > 1000
                        select pi;

            var model = GetQueryModel(query);

            Assert.AreEqual("{ \"nscanned\" : { \"$gt\" : 1000 } }", model.Query.ToJson());
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereSystemProfileInfoTimeStampGreatherThanJan12012(ExecutionTarget target)
        {
            var query = from pi in CreateQueryable<SystemProfileInfo>(_systemProfileCollection)
                        where pi.Timestamp > new DateTime(2012, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        select pi;

            var model = GetQueryModel(query);

            Assert.AreEqual("{ \"ts\" : { \"$gt\" : ISODate(\"2012-01-01T00:00:00Z\") } }", model.Query.ToJson());
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereTripleAnd(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(2, 0, 0))
            {
                // the query is a bit odd in order to force the built query to be promoted to $and form
                var query = from c in CreateQueryable<C>(target)
                            where c.X >= 0 && c.X >= 1 && c.Y == 11
                            select c;

                AssertQueryOrPipeline(query, 2, "{ \"$and\" : [{ \"x\" : { \"$gte\" : 0 } }, { \"x\" : { \"$gte\" : 1 } }, { \"y\" : 11 }] }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereTripleOr(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X == 1 || c.Y == 33 || c.S == "x is 1"
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"$or\" : [{ \"x\" : 1 }, { \"y\" : 33 }, { \"s\" : \"x is 1\" }] }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereWithIndex(ExecutionTarget target)
        {
            var query = CreateQueryable<C>(target).Where((c, i) => true);
            Assert.Throws<MongoLinqException>(
                () => query.ToList(),
                "The indexed version of the Where query operator is not supported.");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X == 1
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"x\" : 1 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXEquals1AndYEquals11(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X == 1 & c.Y == 11
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"x\" : 1, \"y\" : 11 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXEquals1AndAlsoYEquals11(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X == 1 && c.Y == 11
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"x\" : 1, \"y\" : 11 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXEquals1AndYEquals11UsingTwoWhereClauses(ExecutionTarget target)
        {
            // note: using different variable names in the two where clauses to test parameter replacement when combining predicates
            var query = CreateQueryable<C>(target)
                .Where(c => c.X == 1)
                .Where(d => d.Y == 11);

            AssertQueryOrPipeline(query, 1, "{ \"x\" : 1, \"y\" : 11 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXEquals1AndYEquals11Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.X == 1 && c.Y == 11)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"$nor\" : [{ \"x\" : 1, \"y\" : 11 }] }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXEquals1AndYEquals11AndZEquals11(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X == 1 && c.Y == 11 && c.D.Z == 11
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"x\" : 1, \"y\" : 11, \"d.z\" : 11 }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.X == 1)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"x\" : { \"$ne\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXEquals1OrYEquals33(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X == 1 | c.Y == 33
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"$or\" : [{ \"x\" : 1 }, { \"y\" : 33 }] }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXEquals1OrElseYEquals33(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X == 1 || c.Y == 33
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"$or\" : [{ \"x\" : 1 }, { \"y\" : 33 }] }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXEquals1OrYEquals33Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.X == 1 || c.Y == 33)
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"$nor\" : [{ \"x\" : 1 }, { \"y\" : 33 }] }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXEquals1OrYEquals33NotNot(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !!(c.X == 1 || c.Y == 33)
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"$or\" : [{ \"x\" : 1 }, { \"y\" : 33 }] }");
        }

        [Test]
        public void TestWhereXEquals1UsingJavaScript()
        {
            var query = from c in CreateQueryable<C>(ExecutionTarget.Query)
                        where c.X == 1 && Query.Where("this.x < 9").Inject()
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"x\" : 1, \"$where\" : { \"$code\" : \"this.x < 9\" } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXGreaterThan1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X > 1
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"x\" : { \"$gt\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXGreaterThan1AndLessThan3(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X > 1 && c.X < 3
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"x\" : { \"$gt\" : 1, \"$lt\" : 3 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXGreaterThan1AndLessThan3Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.X > 1 && c.X < 3)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"$nor\" : [{ \"x\" : { \"$gt\" : 1, \"$lt\" : 3 } }] }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXGreaterThan1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.X > 1)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"x\" : { \"$not\" : { \"$gt\" : 1 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXGreaterThan1Reversed(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where 1 < c.X
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"x\" : { \"$gt\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXGreaterThanOrEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X >= 1
                        select c;

            AssertQueryOrPipeline(query, 5, "{ \"x\" : { \"$gte\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXGreaterThanOrEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.X >= 1)
                        select c;

            AssertQueryOrPipeline(query, 0, "{ \"x\" : { \"$not\" : { \"$gte\" : 1 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXGreaterThanOrEquals1Reversed(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where 1 <= c.X
                        select c;

            AssertQueryOrPipeline(query, 5, "{ \"x\" : { \"$gte\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXIn1Or9(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X.In(new[] { 1, 9 })
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"x\" : { \"$in\" : [1, 9] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXIn1Or9Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !c.X.In(new[] { 1, 9 })
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"x\" : { \"$nin\" : [1, 9] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXIsTypeInt32(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where Query.Type("x", BsonType.Int32).Inject()
                        select c;

            AssertQueryOrPipeline(query, 5, "{ \"x\" : { \"$type\" : 16 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXIsTypeInt32Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !Query.Type("x", BsonType.Int32).Inject()
                        select c;

            AssertQueryOrPipeline(query, 0, "{ \"x\" : { \"$not\" : { \"$type\" : 16 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXLessThan1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X < 1
                        select c;

            AssertQueryOrPipeline(query, 0, "{ \"x\" : { \"$lt\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXLessThan1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.X < 1)
                        select c;

            AssertQueryOrPipeline(query, 5, "{ \"x\" : { \"$not\" : { \"$lt\" : 1 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXLessThan1Reversed(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where 1 > c.X
                        select c;

            AssertQueryOrPipeline(query, 0, "{ \"x\" : { \"$lt\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXLessThanOrEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X <= 1
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"x\" : { \"$lte\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXLessThanOrEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.X <= 1)
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"x\" : { \"$not\" : { \"$lte\" : 1 } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXLessThanOrEquals1Reversed(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where 1 >= c.X
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"x\" : { \"$lte\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXModOneEquals0AndXModTwoEquals0(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(2, 0, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where (c.X % 1 == 0) && (c.X % 2 == 0)
                            select c;

                AssertQueryOrPipeline(query, 2, "{ \"$and\" : [{ \"x\" : { \"$mod\" : [1, 0] } }, { \"x\" : { \"$mod\" : [2, 0] } }] }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXModOneEquals0AndXModTwoEquals0Not(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(2, 0, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where !((c.X % 1 == 0) && (c.X % 2 == 0))
                            select c;

                AssertQueryOrPipeline(query, 3, "{ \"$nor\" : [{ \"$and\" : [{ \"x\" : { \"$mod\" : [1, 0] } }, { \"x\" : { \"$mod\" : [2, 0] } }] }] }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXModOneEquals0AndXModTwoEquals0NotNot(ExecutionTarget target)
        {
            if (_server.BuildInfo.Version >= new Version(2, 0, 0))
            {
                var query = from c in CreateQueryable<C>(target)
                            where !!((c.X % 1 == 0) && (c.X % 2 == 0))
                            select c;

                AssertQueryOrPipeline(query, 2, "{ \"$or\" : [{ \"$and\" : [{ \"x\" : { \"$mod\" : [1, 0] } }, { \"x\" : { \"$mod\" : [2, 0] } }] }] }");
            }
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXModTwoEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X % 2 == 1
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"x\" : { \"$mod\" : [2, 1] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXModTwoEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.X % 2 == 1)
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"x\" : { \"$not\" : { \"$mod\" : [2, 1] } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXModTwoEquals1Reversed(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where 1 == c.X % 2
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"x\" : { \"$mod\" : [2, 1] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXModTwoNotEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X % 2 != 1
                        select c;

            AssertQueryOrPipeline(query, 2, "{ \"x\" : { \"$not\" : { \"$mod\" : [2, 1] } } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXModTwoNotEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.X % 2 != 1)
                        select c;

            AssertQueryOrPipeline(query, 3, "{ \"x\" : { \"$mod\" : [2, 1] } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXNotEquals1(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where c.X != 1
                        select c;

            AssertQueryOrPipeline(query, 4, "{ \"x\" : { \"$ne\" : 1 } }");
        }

        [Test]
        [ExecutionTargetsTestCaseSourceAttribute]
        public void TestWhereXNotEquals1Not(ExecutionTarget target)
        {
            var query = from c in CreateQueryable<C>(target)
                        where !(c.X != 1)
                        select c;

            AssertQueryOrPipeline(query, 1, "{ \"x\" : 1 }");
        }

        private void AssertQueryOrPipeline<T>(IQueryable<T> query, int count, string queryJson)
        {
            var model = GetExecutionModel(query);

            Assert.AreEqual(typeof(T), model.DocumentType);
            if (model.ModelType == ExecutionModelType.Query)
            {
                var queryModel = (QueryModel)model;
                Assert.IsNull(queryModel.Fields);
                Assert.IsNull(queryModel.NumberToLimit);
                Assert.IsNull(queryModel.NumberToSkip);
                Assert.IsNull(queryModel.SortBy);
                Assert.AreEqual(queryJson, queryModel.Query.ToJson());
            }
            else
            {
                var aggModel = (PipelineModel)model;
                Assert.AreEqual(1, aggModel.Pipeline.Count());
                Assert.AreEqual(queryJson, aggModel.Pipeline.ElementAt(0)["$match"].ToString());
            }

            Assert.AreEqual(count, query.ToList().Count);
        }
    }
}