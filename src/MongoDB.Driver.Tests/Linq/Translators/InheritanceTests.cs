using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MongoDB.Driver.Tests.Linq.Translators
{
    [TestFixture, Category("Linq"), Category("Linq.Inheritance")]
    public class InheritanceTests : LinqTestBase
    {
        private MongoServer _server;
        private MongoDatabase _database;
        private MongoCollection<B> _collection;

        [TestFixtureSetUp]
        public void Setup()
        {
            _server = Configuration.TestServer;
            _database = Configuration.TestDatabase;
            _collection = Configuration.GetTestCollection<B>();

            _collection.Drop();
            _collection.Insert(new B { Id = ObjectId.GenerateNewId(), b = 1 });
            _collection.Insert(new C { Id = ObjectId.GenerateNewId(), b = 2, c = 2 });
            _collection.Insert(new D { Id = ObjectId.GenerateNewId(), b = 3, c = 3, d = 3 });
        }

        [Test]
        public void TestOfTypeB()
        {
            var query = CreateQueryable<B>(_collection).OfType<B>();
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ }", model.Query.ToJson());

            Assert.AreEqual(3, query.ToList().Count);
        }

        [Test]
        public void TestOfTypeC()
        {
            var query = CreateQueryable<B>(_collection).OfType<C>();
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"_t\" : \"C\" }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count);
        }

        [Test]
        public void TestOfTypeCWhereCGreaterThan0()
        {
            var query = CreateQueryable<B>(_collection).OfType<C>().Where(c => c.c > 0);
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"_t\" : \"C\", \"c\" : { \"$gt\" : 0 } }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count); // should match 2 but for that you need to use the hierarchical discriminator
        }

        [Test]
        public void TestOfTypeD()
        {
            var query = CreateQueryable<B>(_collection).OfType<D>();
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"_t\" : \"D\" }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count);
        }

        [Test]
        public void TestOfTypeDWithProjection()
        {
            var query = CreateQueryable<B>(_collection).OfType<D>().Select(x => new { A = x.d });
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.AreEqual("{ \"d\" : 1, \"_id\" : 0 }", model.Fields.ToJson());
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"_t\" : \"D\" }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count);
        }

        [Test]
        public void TestWhereBGreaterThan0OfTypeCWhereCGreaterThan0()
        {
            var query = CreateQueryable<B>(_collection).Where(b => b.b > 0).OfType<C>().Where(c => c.c > 0);
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"b\" : { \"$gt\" : 0 }, \"_t\" : \"C\", \"c\" : { \"$gt\" : 0 } }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count);
        }

        [Test]
        public void TestWhereBIsB()
        {
            var query =
                from b in CreateQueryable<B>(_collection)
                where b is B
                select b;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ }", model.Query.ToJson());

            Assert.AreEqual(3, query.ToList().Count);
        }

        [Test]
        public void TestWhereBIsC()
        {
            var query =
                from b in CreateQueryable<B>(_collection)
                where b is C
                select b;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"_t\" : \"C\" }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count); // should match 2 but for that you need to use the hierarchical discriminator
        }

        [Test]
        public void TestWhereBIsD()
        {
            var query =
                from b in CreateQueryable<B>(_collection)
                where b is D
                select b;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"_t\" : \"D\" }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count);
        }

        [Test]
        public void TestWhereBTypeEqualsB()
        {
            var query =
                from b in CreateQueryable<B>(_collection)
                where b.GetType() == typeof(B)
                select b;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ }", model.Query.ToJson());

            Assert.AreEqual(3, query.ToList().Count);
        }

        [Test]
        public void TestWhereBTypeEqualsC()
        {
            var query =
                from b in CreateQueryable<B>(_collection)
                where b.GetType() == typeof(C)
                select b;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"_t.0\" : { \"$exists\" : false }, \"_t\" : \"C\" }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count);
        }

        [Test]
        public void TestWhereBTypeEqualsD()
        {
            var query =
                from b in CreateQueryable<B>(_collection)
                where b.GetType() == typeof(D)
                select b;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(B), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"_t.0\" : { \"$exists\" : false }, \"_t\" : \"D\" }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count);
        }

        private class B
        {
            public ObjectId Id;
            public int b;
        }

        private class C : B
        {
            public int c;
        }

        private class D : C
        {
            public int d;
        }
    }
}