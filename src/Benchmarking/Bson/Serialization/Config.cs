using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Benchmarking.Bson.Serialization
{
    public enum ConfigVariant
    {
        Small,
        Large
    }

    public class Config
    {
        private readonly byte[] _bytes;
        private readonly BsonDocument _document;
        private readonly IBsonSerializer<BsonDocument> _documentSerializer;
        private readonly object _object;
        private readonly IBsonSerializer<SmallPoco> _objectSerializer;

        public Config(ConfigVariant variant)
        {
            _documentSerializer = BsonDocumentSerializer.Instance;
            _objectSerializer = BsonSerializer.LookupSerializer<SmallPoco>();

            switch (variant)
            {
                case ConfigVariant.Small:
                    _document = new BsonDocument
                    {
                        {  "_id", ObjectId.GenerateNewId() },
                        { "a", 1 },
                        { "b", "b" }
                    };

                    _bytes = _document.ToBson();
                    _object = BsonSerializer.Deserialize<SmallPoco>(_document);
                    break;
                case ConfigVariant.Large:
                    _document = new BsonDocument
                    {
                        {  "_id", ObjectId.GenerateNewId() },
                        { "alpha", 1 },
                        { "beta", "beta" },
                        { "chi", new BsonDocument
                            {
                                { "delta", 30L },
                                { "epsilon", new BsonArray { 20, 30, 40 } }
                            }
                        },
                        { "gamma", new BsonArray
                            {
                                new BsonDocument
                                {
                                    { "zeta", "semi long string that is getting longer the more I type" },
                                    { "really long field name that should never exist in a document", 20.43 }
                                },
                                new BsonDocument
                                {
                                    { "zeta", "semi long string that is getting longer the more I type" },
                                    { "really long field name that should never exist in a document", 48.23 }
                                },
                            }
                        }
                    };

                    _bytes = _document.ToBson();
                    _object = BsonSerializer.Deserialize<LargePoco>(_document);
                    break;
            }
        }

        public byte[] Bytes => _bytes;
        public BsonDocument Document => _document;
        public IBsonSerializer<BsonDocument> DocumentSerializer => _documentSerializer;
        public object Object => _object;
        public IBsonSerializer<SmallPoco> ObjectSerializer => _objectSerializer;

        public class SmallPoco
        {
            public ObjectId Id { get; set; }
            [BsonElement("a")]
            public int A { get; set; }
            [BsonElement("b")]
            public string B { get; set; }
        }

        public class LargePoco
        {
            public ObjectId Id { get; set; }
            [BsonElement("alpha")]
            public string Alpha { get; set; }
            [BsonElement("beta")]
            public int Beta { get; set; }
            [BsonElement("chi")]
            public LargePoco_Chi Chi { get; set; }
            [BsonElement("gamma")]
            public LargePoco_Gamma[] Gamma { get; set; }
        }

        public class LargePoco_Chi
        {
            [BsonElement("delta")]
            public long Delta { get; set; }
            [BsonElement("epsilon")]
            public int[] Epsilon { get; set; }
        }

        public class LargePoco_Gamma
        {
            [BsonElement("zeta")]
            public string Zeta { get; set; }
            [BsonElement("really long field name that should never exist in a document")]
            public double LongFieldName { get; set; }
        }


    }
}
