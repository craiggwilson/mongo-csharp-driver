using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using NUnit.Framework;

namespace MongoDB.Driver.Tests.Linq
{
    using MongoDB.Driver.Linq;

    [TestFixture]
    [Category("Linq")]
    public class PipelineModelTests : LinqTestBase
    {
        private class District
        {
            public string Name { get; set; }

            public int? Population { get; set; }
        }

        private class ZipInfo
        {
            public ObjectId Id { get; set; }

            public string State { get; set; }

            public string City { get; set; }

            public string ZipCode { get; set; }

            public int Pop { get; set; }

            public List<District> Districts { get; set; }
        }
        
        [SetUp]
        public void SetUp()
        {
            var collection = Configuration.TestDatabase.GetCollection<ZipInfo>("zipinfos");
            collection.RemoveAll();

            collection.Save(new ZipInfo { State = "SA", City = "SAC1", ZipCode = "SAC1Z1", Pop = 10, Districts = new[] { new District { Name = "A", Population = 10 } }.ToList() });
            collection.Save(new ZipInfo { State = "SA", City = "SAC1", ZipCode = "SAC1Z2", Pop = 20, Districts = new[] { new District { Name = "B", Population = 20 } }.ToList() });
            collection.Save(new ZipInfo { State = "SA", City = "SAC2", ZipCode = "SAC2Z1", Pop = 100, Districts = new[] { new District { Name = "C", Population = 100 } }.ToList() });
            collection.Save(new ZipInfo { State = "SB", City = "SBC1", ZipCode = "SBC1Z1", Pop = 10, Districts = new[] { new District { Name = "D", Population = 10 } }.ToList() });
            collection.Save(new ZipInfo { State = "SB", City = "SBC2", ZipCode = "SBC2Z1", Pop = 20, Districts = new[] { new District { Name = "E", Population = 20 } }.ToList() });

            var mongoDocs = Configuration.TestDatabase.GetCollection<MongoDocument>("mongoDocs");
            mongoDocs.RemoveAll();

            mongoDocs.Save(
                new MongoDocument(
                    new BsonDocument(
                        new BsonElement("_id", ObjectId.GenerateNewId()),
                        new BsonElement("IsEnabled", true))));

            mongoDocs.Save(
                new MongoDocument(
                    new BsonDocument(
                        new BsonElement("_id", ObjectId.GenerateNewId()),
                        new BsonElement("IsEnabled", false))));
        }

        [Test]
        public void TestZipInfoQuery1()
        {
            var zipInfos = Configuration.TestDatabase.GetCollection<ZipInfo>("zipinfos");

            var first = from zi in CreateQueryable<ZipInfo>(zipInfos)
                        group zi by new { zi.State, zi.City } into g
                        select new { State = g.Key.State, City = g.Key.City, Pop = g.Sum(x => x.Pop) };

            var query = from f in first
                        orderby f.Pop, f.State, f.City
                        group f by f.State into g
                        orderby g.Key
                        select new
                        {
                            State = g.Key,
                            BiggestCity = new { Name = g.Select(x => x.City).Last(), Population = g.Select(x => x.Pop).Last() },
                            SmallestCity = new { Name = g.Select(x => x.City).First(), Population = g.Select(x => x.Pop).First() }
                        };

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : { \"State\" : \"$State\", \"City\" : \"$City\" }, \"_agg0\" : { \"$sum\" : \"$Pop\" } } }",
                "{ \"$project\" : { \"State\" : \"$_id.State\", \"City\" : \"$_id.City\", \"Pop\" : \"$_agg0\", \"_id\" : 0 } }",
                "{ \"$sort\" : { \"Pop\" : 1, \"State\" : 1, \"City\" : 1 } }",
                "{ \"$group\" : { \"_id\" : \"$State\", \"_agg0\" : { \"$last\" : \"$City\" }, \"_agg1\" : { \"$last\" : \"$Pop\" }, \"_agg2\" : { \"$first\" : \"$City\" }, \"_agg3\" : { \"$first\" : \"$Pop\" } } }",
                "{ \"$sort\" : { \"_id\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("SAC2", results[0].BiggestCity.Name);
            Assert.AreEqual(100, results[0].BiggestCity.Population);
            Assert.AreEqual("SAC1", results[0].SmallestCity.Name);
            Assert.AreEqual(30, results[0].SmallestCity.Population);
        }

        [Test]
        public void TestZipInfoQuery2()
        {
            var zipInfos = Configuration.TestDatabase.GetCollection<ZipInfo>("zipinfos");
            var query = CreateQueryable<ZipInfo>(zipInfos)
                .GroupBy(x => new { x.State, x.City })
                .Select(g => new { g.Key.State, g.Key.City, Pop = g.Sum(i => i.Pop) })
                .OrderBy(x => x.Pop).ThenBy(x => x.State).ThenBy(x => x.City)
                .GroupBy(x => x.State)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    State = g.Key,
                    BiggestCity = new { Name = g.Select(x => x.City).Last(), Population = g.Select(x => x.Pop).Last() },
                    SmallestCity = new { Name = g.Select(x => x.City).First(), Population = g.Select(x => x.Pop).First() }
                });

            var model = GetPipelineModel(query);
            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : { \"State\" : \"$State\", \"City\" : \"$City\" }, \"_agg0\" : { \"$sum\" : \"$Pop\" } } }",
                "{ \"$project\" : { \"State\" : \"$_id.State\", \"City\" : \"$_id.City\", \"Pop\" : \"$_agg0\", \"_id\" : 0 } }",
                "{ \"$sort\" : { \"Pop\" : 1, \"State\" : 1, \"City\" : 1 } }",
                "{ \"$group\" : { \"_id\" : \"$State\", \"_agg0\" : { \"$last\" : \"$City\" }, \"_agg1\" : { \"$last\" : \"$Pop\" }, \"_agg2\" : { \"$first\" : \"$City\" }, \"_agg3\" : { \"$first\" : \"$Pop\" } } }",
                "{ \"$sort\" : { \"_id\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("SAC2", results[0].BiggestCity.Name);
            Assert.AreEqual(100, results[0].BiggestCity.Population);
            Assert.AreEqual("SAC1", results[0].SmallestCity.Name);
            Assert.AreEqual(30, results[0].SmallestCity.Population);
        }

        [Test]
        public void TestZipInfoQuery3()
        {
            var zipInfos = Configuration.TestDatabase.GetCollection<ZipInfo>("zipinfos");

            var query = from zi in CreateQueryable<ZipInfo>(zipInfos)
                        group zi by new { zi.State, zi.City } into g
                        orderby g.Sum(x => x.Pop), g.Key.State, g.Key.City
                        group g by g.Key.State into g2
                        orderby g2.Key
                        select new
                        {
                            State = g2.Key,
                            BiggestCity = new { Name = g2.Select(x => x.Key.City).Last(), Population = g2.Select(x => x.Sum(y => y.Pop)).Last() },
                            SmallestCity = new { Name = g2.Select(x => x.Key.City).First(), Population = g2.Select(x => x.Sum(y => y.Pop)).First() }
                        };

            var model = GetPipelineModel(query);

            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : { \"State\" : \"$State\", \"City\" : \"$City\" }, \"_agg0\" : { \"$sum\" : \"$Pop\" } } }",
                "{ \"$sort\" : { \"_agg0\" : 1, \"_id.State\" : 1, \"_id.City\" : 1 } }",
                "{ \"$group\" : { \"_id\" : \"$_id.State\", \"_agg0\" : { \"$last\" : \"$_id.City\" }, \"_agg1\" : { \"$last\" : \"$_agg0\" }, \"_agg2\" : { \"$first\" : \"$_id.City\" }, \"_agg3\" : { \"$first\" : \"$_agg0\" } } }",
                "{ \"$sort\" : { \"_id\" : 1 } }");

            var results = query.ToList();
            Assert.AreEqual(2, results.Count);
            Assert.AreEqual("SAC2", results[0].BiggestCity.Name);
            Assert.AreEqual(100, results[0].BiggestCity.Population);
            Assert.AreEqual("SAC1", results[0].SmallestCity.Name);
            Assert.AreEqual(30, results[0].SmallestCity.Population);
        }
        
        [Test]
        public void TestZipInfoQuery4()
        {
            var zipInfos = Configuration.TestDatabase.GetCollection<ZipInfo>("zipinfos");
            var query = CreateQueryable<ZipInfo>(zipInfos)
                .GroupBy(x => new { x.State, x.City })
                .Select(g => new { g.Key.State, g.Key.City, Pop = g.Sum(i => i.Pop) })
                .Where(x => x.Pop > 100)
                .GroupBy(x => x.State)
                .Select(g => new { g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);

            var model = GetPipelineModel(query);
            AssertPipeline(model.Pipeline,
                "{ \"$group\" : { \"_id\" : { \"State\" : \"$State\", \"City\" : \"$City\" }, \"_agg0\" : { \"$sum\" : \"$Pop\" } } }",
                "{ \"$project\" : { \"State\" : \"$_id.State\", \"City\" : \"$_id.City\", \"Pop\" : \"$_agg0\", \"_id\" : 0 } }",
                "{ \"$match\" : { \"Pop\" : { \"$gt\" : 100 } } }",
                "{ \"$group\" : { \"_id\" : \"$State\", \"_agg0\" : { \"$sum\" : 1 } } }",
                "{ \"$project\" : { \"Key\" : \"$_id\", \"Count\" : \"$_agg0\", \"_id\" : 0 } }", 
                "{ \"$sort\" : { \"Count\" : -1 } }");
        }

        [Test]
        public void TestBsonDocumentBackedClass_LooselyTyped_Member()
        {
            // This test can be made to pass/fail by changing the register member call in mongodocumentclassserializer
            // from BooleanSerializer to BsonValueSerializer
            var mongoDocs = Configuration.TestDatabase.GetCollection<MongoDocument>("mongoDocs");
            var query = CreateQueryable<MongoDocument>(mongoDocs, ExecutionTarget.Pipeline);

            query = query.Where(o => o["IsArchived"] == false);

            var model = this.GetPipelineModel(query);
        }

        [Test]
        public void TestBsonDocumentBackedClass_StronglyTyped_Member()
        { 
            // This test can be made to pass/fail by changing the register member call in mongodocumentclassserializer
            // from BooleanSerializer to BsonValueSerializer
            var mongoDocs = Configuration.TestDatabase.GetCollection<MongoDocument>("mongoDocs");
            var query = CreateQueryable<MongoDocument>(mongoDocs, ExecutionTarget.Pipeline);

            query = query.Where(o => o.IsArchived == false);

            var model = this.GetPipelineModel(query);
        }

        [Test]
        public void TestComplexArrayOrderingProjection()
        {
            var zipInfos = Configuration.TestDatabase.GetCollection<ZipInfo>("zipinfos");
            var query = CreateQueryable<ZipInfo>(zipInfos, ExecutionTarget.Pipeline);

            var projection =
                query.Select(
                    o => new { Data = o.Districts.OrderBy(c => c.Population).Select(c => new { Size = c.Population }) });

            var model = this.GetPipelineModel(projection);
        }
    }
}