namespace MongoDB.Driver.Tests.Linq
{
    using System;
    using System.Collections.Generic;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Attributes;
    
    [Serializable]
    [BsonSerializer(typeof(MongoDocumentClassSerializer))]
    public class MongoDocument : BsonDocumentBackedClass
    {
        public MongoDocument()
            : base(new MongoDocumentClassSerializer())
        {
        }

        public MongoDocument(BsonDocument backingDocument)
            : base(backingDocument, new MongoDocumentClassSerializer())
        {
        }

        public MongoDocument(BsonDocument backingDocument, IBsonDocumentSerializer serializer)
            : base(backingDocument, serializer)
        {
        }
        
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        public bool IsArchived { get; set; }
        
        public BsonValue this[string fieldname]
        {
            get
            {
                if (this.BackingDocument.Contains(fieldname))
                {
                    return this.BackingDocument[fieldname];
                }

                return BsonNull.Value;
            }

            set
            {
                this.BackingDocument[fieldname] = value;
            }
        }

        public static MongoDocument Parse(string json)
        {
            var doc = (json != null && json != "null") ? BsonDocument.Parse(json) : new BsonDocument();
            return new MongoDocument(doc);
        }

        public BsonDocument GetBackingDocumentForUpdate()
        {
            var backingDocument = this.BackingDocument;
            return backingDocument;
        }

        public BsonDocument GetBackingDocument()
        {
            var backingDocument = this.BackingDocument;
            return backingDocument;
        }
    }
}
