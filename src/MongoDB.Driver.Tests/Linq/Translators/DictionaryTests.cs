/* Copyright 2010-2012 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Driver.Tests.Linq.Translators
{
    [TestFixture, Category("Linq")]
    public class DictionaryTests : LinqTestBase
    {
        private MongoServer _server;
        private MongoDatabase _database;
        private MongoCollection _collection;

        [TestFixtureSetUp]
        public void Setup()
        {
            _server = Configuration.TestServer;
            _database = Configuration.TestDatabase;
            _collection = Configuration.GetTestCollection<C>();

            var de = new Dictionary<string, int>();
            var dx = new Dictionary<string, int>() { { "x", 1 } };
            var dy = new Dictionary<string, int>() { { "y", 1 } };

            var he = new Hashtable();
            var hx = new Hashtable { { "x", 1 } };
            var hy = new Hashtable { { "y", 1 } };

            _collection.Drop();
            _collection.Insert(new C { D = null, E = null, F = null,  H = null, I = null, J = null });
            _collection.Insert(new C { D = de, E = de, F = de, H = he, I = he, J = he });
            _collection.Insert(new C { D = dx, E = dx, F = dx, H = hx, I = hx, J = hx });
            _collection.Insert(new C { D = dy, E = dy, F = dy, H = hy, I = hy, J = hy });
        }

        [Test]
        public void TestWhereDContainsKeyX()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.D.ContainsKey("x")
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"D.x\" : { \"$exists\" : true } }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count());
        }

        [Test]
        public void TestWhereDContainsKeyZ()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.D.ContainsKey("z")
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"D.z\" : { \"$exists\" : true } }", model.Query.ToJson());

            Assert.AreEqual(0, query.ToList().Count());
        }

        [Test]
        public void TestWhereEContainsKeyX()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.E.ContainsKey("x")
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"E.k\" : \"x\" }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count());
        }

        [Test]
        public void TestWhereEContainsKeyZ()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.E.ContainsKey("z")
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"E.k\" : \"z\" }", model.Query.ToJson());

            Assert.AreEqual(0, query.ToList().Count());
        }

        [Test]
        public void TestWhereFContainsKeyX()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.F.ContainsKey("x")
                        select c;

            Assert.Throws<MongoLinqException>(() => { GetQueryModel(query); });
        }

        [Test]
        public void TestWhereHContainsKeyX()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.H.Contains("x")
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"H.x\" : { \"$exists\" : true } }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count());
        }

        [Test]
        public void TestWhereHContainsKeyZ()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.H.Contains("z")
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"H.z\" : { \"$exists\" : true } }", model.Query.ToJson());

            Assert.AreEqual(0, query.ToList().Count());
        }

        [Test]
        public void TestWhereIContainsKeyX()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.I.Contains("x")
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"I.k\" : \"x\" }", model.Query.ToJson());

            Assert.AreEqual(1, query.ToList().Count());
        }

        [Test]
        public void TestWhereIContainsKeyZ()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.I.Contains("z")
                        select c;
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.SortBy);
            Assert.AreEqual("{ \"I.k\" : \"z\" }", model.Query.ToJson());

            Assert.AreEqual(0, query.ToList().Count());
        }

        [Test]
        public void TestWhereJContainsKeyX()
        {
            var query = from c in CreateQueryable<C>(_collection)
                        where c.J.Contains("x")
                        select c;

            Assert.Throws<MongoLinqException>(() => { GetQueryModel(query); });
        }

        private class C
        {
            public ObjectId Id { get; set; }

            [BsonDictionaryOptions(DictionaryRepresentation.Document)]
            public IDictionary<string, int> D { get; set; } // serialized as { D : { x : 1, ... } }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            public IDictionary<string, int> E { get; set; } // serialized as { E : [{ k : "x", v : 1 }, ...] }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
            public IDictionary<string, int> F { get; set; } // serialized as { F : [["x", 1], ... ] }

            [BsonDictionaryOptions(DictionaryRepresentation.Document)]
            public IDictionary H { get; set; } // serialized as { H : { x : 1, ... } }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
            public IDictionary I { get; set; } // serialized as { I : [{ k : "x", v : 1 }, ...] }
            [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
            public IDictionary J { get; set; } // serialized as { J : [["x", 1], ... ] }
        }
    }
}
