using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Driver.Tests.Linq.Translators
{
    [TestFixture, Category("Linq")]
    public class NullableTests : LinqTestBase
    {
        private MongoServer _server;
        private MongoDatabase _database;
        private MongoCollection<C> _collection;

        [TestFixtureSetUp]
        public void Setup()
        {
            _server = Configuration.TestServer;
            _database = Configuration.TestDatabase;
            _collection = Configuration.GetTestCollection<C>();

            _collection.Drop();
            _collection.Insert(new C { E = null });
            _collection.Insert(new C { E = E.A });
            _collection.Insert(new C { E = E.B });
            _collection.Insert(new C { X = null });
            _collection.Insert(new C { X = 1 });
            _collection.Insert(new C { X = 2 });
        }

        [Test]
        public void TestWhereEEqualsA()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.E == E.A
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"e\" : \"A\" }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count);
        }

        [Test]
        public void TestWhereEEqualsNull()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.E == null
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"e\" : null }", model.Query.ToJson());

            Assert.AreEqual(4, query.ToList().Count);
        }

        [Test]
        public void TestWhereXEquals1()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.X == 1
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"x\" : 1 }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count);
        }

        [Test]
        public void TestWhereXEqualsNull()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.X == null
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"x\" : null }", model.Query.ToJson());

            Assert.AreEqual(4, query.ToList().Count);
        }

        private enum E { None, A, B };

        private class C
        {
            public ObjectId Id { get; set; }
            [BsonElement("e")]
            [BsonRepresentation(BsonType.String)]
            public E? E { get; set; }
            [BsonElement("x")]
            public int? X { get; set; }
        }
    }
}