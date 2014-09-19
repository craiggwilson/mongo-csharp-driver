namespace MongoDB.Driver.Tests.Linq
{
    using System;
    using System.Collections.Generic;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Attributes;
    
    public enum AccessType
    {
        Unknown,
        Allow,
        ReadOnly,
        Deny
    }

    public enum SourceType
    {
        Unknown,
        Parent,
        Record,
        App
    }

    public enum ActorType
    {
        Unknown = 0,
        Team = 1,
        User = 2,
        Owner = 3,
        All = 4
    }

    public class AccessRight
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.Int32)]
        public AccessType AccessType { get; set; }

        [BsonRepresentation(BsonType.Int32)]
        public SourceType SourceType { get; set; }

        [BsonRepresentation(BsonType.Int32)]
        public ActorType ActorType { get; set; }

        public string ActorId { get; set; }

        public string ParentId { get; set; }

        public bool Permissive { get; set; }
    }

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

        public List<AccessRight> AccessRights { get; set; }
        
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
