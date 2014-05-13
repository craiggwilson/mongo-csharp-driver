using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoDB.Driver.Tests.Linq
{
    [TestFixture, Category("Linq")]
    public class QueryModelBuilderTests : LinqIntegrationTestBase
    {
        [Test]
        public void TestRaw()
        {
            var query = CreateQueryable<C>();
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.Query);
            Assert.IsNull(model.SortBy);
        }

        [Test]
        public void TestOrderBy()
        {
            var query = CreateQueryable<C>().OrderBy(x => x.B);
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.Query);
            Assert.AreEqual("{ \"b\" : 1 }", model.SortBy.ToJson());
        }

        [Test]
        public void TestOrderByThenByWhereSkipTake()
        {
            var query = CreateQueryable<C>()
                .OrderBy(x => x.B)
                .ThenBy(x => x.S)
                .Where(x => x.D.Z == 10)
                .Skip(5)
                .Take(10);

            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.AreEqual(10, model.NumberToLimit);
            Assert.AreEqual(5, model.NumberToSkip);
            Assert.AreEqual("{ \"d.z\" : 10 }", model.Query.ToJson());
            Assert.AreEqual("{ \"b\" : 1, \"s\" : 1 }", model.SortBy.ToJson());
        }

        [Test]
        public void TestOrderByThenByWhereOrderBy()
        {
            var query = CreateQueryable<C>()
                .OrderBy(x => x.B)
                .ThenBy(x => x.S)
                .Where(x => x.S == "Yay")
                .OrderBy(x => x.S);

            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.AreEqual("{ \"s\" : \"Yay\" }", model.Query.ToJson());
            Assert.AreEqual("{ \"s\" : 1, \"b\" : 1 }", model.SortBy.ToJson());
        }

        [Test]
        public void TestOrderByThenByWhereOrderByDescending()
        {
            var query = CreateQueryable<C>()
                .OrderBy(x => x.B)
                .ThenBy(x => x.S)
                .Where(x => x.B == true)
                .OrderByDescending(x => x.X);

            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.AreEqual("{ \"b\" : true }", model.Query.ToJson());
            Assert.AreEqual("{ \"x\" : -1, \"b\" : 1, \"s\" : 1 }", model.SortBy.ToJson());
        }

        [Test]
        public void TestSelect()
        {
            var query = CreateQueryable<C>().Select(x => new { Some = x.B });
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.AreEqual("{ \"b\" : 1, \"_id\" : 0 }", model.Fields.ToJson());
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.Query);
            Assert.IsNull(model.SortBy);
        }

        [Test]
        public void TestSelectThenWhere()
        {
            var query = CreateQueryable<C>()
                .Select(x => new { Some = x.B })
                .Where(x => x.Some);

            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.AreEqual("{ \"b\" : 1, \"_id\" : 0 }", model.Fields.ToJson());
            Assert.IsNull(model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.AreEqual("{ \"b\" : true }", model.Query.ToJson());
            Assert.IsNull(model.SortBy);
        }

        [Test]
        public void TestSkip()
        {
            var query = CreateQueryable<C>().Skip(5);
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.IsNull(model.NumberToLimit);
            Assert.AreEqual(5, model.NumberToSkip);
            Assert.IsNull(model.Query);
            Assert.IsNull(model.SortBy);
        }

        [Test]
        public void TestSkipAndTake()
        {
            var query = CreateQueryable<C>().Skip(5).Take(10);
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.AreEqual(10, model.NumberToLimit);
            Assert.AreEqual(5, model.NumberToSkip);
            Assert.IsNull(model.Query);
            Assert.IsNull(model.SortBy);
        }

        [Test]
        public void TestTake()
        {
            var query = CreateQueryable<C>().Take(10);
            var model = GetQueryModel(query);

            Assert.AreEqual(typeof(C), model.DocumentType);
            Assert.IsNull(model.Fields);
            Assert.AreEqual(10, model.NumberToLimit);
            Assert.IsNull(model.NumberToSkip);
            Assert.IsNull(model.Query);
            Assert.IsNull(model.SortBy);
        }
    }
}