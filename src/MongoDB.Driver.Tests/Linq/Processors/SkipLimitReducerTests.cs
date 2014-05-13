using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MongoDB.Driver.Tests.Linq.Processors
{
    [TestFixture, Category("Linq"), Category("Linq.SkipLimit")]
    public class SkipLimitReducerTests : LinqTestBase
    {
        private class Entity
        { }

        [Test]
        public void TestSkipThenSkip()
        {
            var query = CreateQueryable<Entity>(Configuration.TestCollection)
                .Skip(5)
                .Skip(5);

            var model = GetQueryModel(query);

            var query2 = CreateQueryable<Entity>(Configuration.TestCollection)
                .Skip(10)
                .Skip(10);

            var model2 = GetQueryModel(query2);

            Assert.AreEqual(10, model.NumberToSkip);
            Assert.IsNull(model.NumberToLimit);
        }

        [Test]
        public void TestSkipThenTake()
        {
            var query = CreateQueryable<Entity>(Configuration.TestCollection)
                .Skip(5)
                .Take(5);

            var model = GetQueryModel(query);

            Assert.AreEqual(5, model.NumberToSkip);
            Assert.AreEqual(5, model.NumberToLimit);
        }

        [Test]
        public void TestSkipThenTakeThenSkip()
        {
            var query = CreateQueryable<Entity>(Configuration.TestCollection)
                .Skip(5)
                .Take(20)
                .Skip(10);

            var model = GetQueryModel(query);

            Assert.AreEqual(15, model.NumberToSkip);
            Assert.AreEqual(10, model.NumberToLimit);
        }

        [Test]
        public void TestSkipThenTakeThenSkipTooMany()
        {
            var query = CreateQueryable<Entity>(Configuration.TestCollection)
                .Skip(5)
                .Take(20)
                .Skip(30);

            var model = GetQueryModel(query);

            Assert.AreEqual(0, model.NumberToSkip);
            Assert.AreEqual(0, model.NumberToLimit);
        }

        [Test]
        public void TestSkipThenTakeThenTake()
        {
            var query = CreateQueryable<Entity>(Configuration.TestCollection)
                .Skip(10)
                .Take(20)
                .Take(5);

            var model = GetQueryModel(query);

            Assert.AreEqual(10, model.NumberToSkip);
            Assert.AreEqual(5, model.NumberToLimit);
        }

        [Test]
        public void TestTakeThenSkip()
        {
            var query = CreateQueryable<Entity>(Configuration.TestCollection)
                .Take(20)
                .Skip(10);

            var model = GetQueryModel(query);

            Assert.AreEqual(10, model.NumberToSkip);
            Assert.AreEqual(10, model.NumberToLimit);
        }

        [Test]
        public void TestTakeThenTakeWithLess()
        {
            var query = CreateQueryable<Entity>(Configuration.TestCollection)
                .Take(20)
                .Take(5);

            var model = GetQueryModel(query);

            Assert.IsNull(model.NumberToSkip);
            Assert.AreEqual(5, model.NumberToLimit);
        }

        [Test]
        public void TestTakeThenTakeWithMore()
        {
            var query = CreateQueryable<Entity>(Configuration.TestCollection)
                .Take(5)
                .Take(20);

            var model = GetQueryModel(query);

            Assert.IsNull(model.NumberToSkip);
            Assert.AreEqual(5, model.NumberToLimit);
        }
    }
}