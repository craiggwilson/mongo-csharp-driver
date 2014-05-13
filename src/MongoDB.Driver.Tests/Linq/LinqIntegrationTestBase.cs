using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Driver.Tests.Linq
{
    [TestFixture]
    public abstract class LinqIntegrationTestBase : LinqTestBase
    {
        protected MongoServer _server;
        protected MongoDatabase _database;
        protected MongoCollection<C> _collection;
        protected MongoCollection<SystemProfileInfo> _systemProfileCollection;

        protected ObjectId _id1 = ObjectId.GenerateNewId();
        protected ObjectId _id2 = ObjectId.GenerateNewId();
        protected ObjectId _id3 = ObjectId.GenerateNewId();
        protected ObjectId _id4 = ObjectId.GenerateNewId();
        protected ObjectId _id5 = ObjectId.GenerateNewId();

        [TestFixtureSetUp]
        public void Setup()
        {
            _server = Configuration.TestServer;
            _server.Connect();
            _database = Configuration.TestDatabase;
            _collection = Configuration.GetTestCollection<C>();
            _systemProfileCollection = _database.GetCollection<SystemProfileInfo>("system.profile");

            // documents inserted deliberately out of order to test sorting
            _collection.Drop();
            _collection.Insert(new C { Id = _id2, X = 2, LX = 2, Y = 11, Date = new DateTime(2000, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc), D = new D { Z = 22 }, NullableDouble = 2, A = new[] { 2, 3, 4 }, DA = new List<D> { new D { Y = 11, Z = 111 }, new D { Z = 222 } }, L = new List<int> { 2, 3, 4 } });
            _collection.Insert(new C { Id = _id1, X = 1, LX = 1, Y = 11, Date = new DateTime(2000, 2, 2, 2, 2, 2, 2, DateTimeKind.Utc), D = new D { Z = 11 }, NullableDouble = 2, S = "abc", SA = new string[] { "Tom", "Dick", "Harry" } });
            _collection.Insert(new C { Id = _id3, X = 3, LX = 3, Y = 33, Date = new DateTime(2001, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc), D = new D { Z = 33 }, NullableDouble = 5, B = true, BA = new bool[] { true }, E = E.A, EA = new E[] { E.A, E.B } });
            _collection.Insert(new C { Id = _id5, X = 5, LX = 5, Y = 44, Date = new DateTime(2001, 2, 2, 2, 2, 2, 2, DateTimeKind.Utc), D = new D { Z = 55 }, DBRef = new MongoDBRef("db", "c", 1), F = new F { G = new G { H = 10 } } });
            _collection.Insert(new C { Id = _id4, X = 4, LX = 4, Y = 44, Date = new DateTime(2001, 3, 3, 3, 3, 3, 3, DateTimeKind.Utc), D = new D { Z = 44 }, S = "   xyz   ", DA = new List<D> { new D { Y = 33, Z = 333 }, new D { Y = 44, Z = 444 } } });
        }

        protected IQueryable<T> CreateQueryable<T>()
        {
            return CreateQueryable<T>(_collection);
        }

        protected IQueryable<T> CreateQueryable<T>(ExecutionTarget target)
        {
            return CreateQueryable<T>(_collection, target);
        }

        protected enum E
        {
            None,
            A,
            B,
            C
        }

        protected class C
        {
            public ObjectId Id { get; set; }
            [BsonElement("x")]
            public int X { get; set; }
            [BsonElement("lx")]
            public long LX { get; set; }
            [BsonElement("y")]
            public int Y { get; set; }
            [BsonElement("d")]
            public D D { get; set; }
            [BsonElement("da")]
            public List<D> DA { get; set; }
            [BsonElement("s")]
            [BsonIgnoreIfNull]
            public string S { get; set; }
            [BsonElement("a")]
            [BsonIgnoreIfNull]
            public int[] A { get; set; }
            [BsonElement("b")]
            public bool B { get; set; }
            [BsonElement("l")]
            [BsonIgnoreIfNull]
            public List<int> L { get; set; }
            [BsonElement("dbref")]
            [BsonIgnoreIfNull]
            public MongoDBRef DBRef { get; set; }
            [BsonElement("e")]
            [BsonIgnoreIfDefault]
            [BsonRepresentation(BsonType.String)]
            public E E { get; set; }
            [BsonElement("ea")]
            [BsonIgnoreIfNull]
            public E[] EA { get; set; }
            [BsonElement("f")]
            public F F { get; set; }
            [BsonElement("sa")]
            [BsonIgnoreIfNull]
            public string[] SA { get; set; }
            [BsonElement("ba")]
            [BsonIgnoreIfNull]
            public bool[] BA { get; set; }
            [BsonElement("date")]
            public DateTime Date { get; set; }
            [BsonElement("nuldub")]
            public double? NullableDouble { get; set; }
        }

        protected class D
        {
            [BsonElement("y")]
            [BsonIgnoreIfNull]
            [BsonDefaultValue(20)]
            public int? Y;
            [BsonElement("z")]
            public int Z; // use field instead of property to test fields also

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType()) { return false; }
                return Z == ((D)obj).Z;
            }

            public override int GetHashCode()
            {
                return Z.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("new D {{ Y = {0}, Z = {1} }}", Y, Z);
            }
        }

        protected class F
        {
            [BsonElement("g")]
            public G G { get; set; }
        }

        protected class G
        {
            [BsonElement("h")]
            public int H { get; set; }
        }

        // used to test some query operators that have an IEqualityComparer parameter
        protected class CEqualityComparer : IEqualityComparer<C>
        {
            public bool Equals(C x, C y)
            {
                return x.Id.Equals(y.Id) && x.X.Equals(y.X) && x.Y.Equals(y.Y);
            }

            public int GetHashCode(C obj)
            {
                return obj.GetHashCode();
            }
        }

        // used to test some query operators that have an IEqualityComparer parameter
        protected class Int32EqualityComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y)
            {
                return x == y;
            }

            public int GetHashCode(int obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
