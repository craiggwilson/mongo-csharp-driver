namespace MongoDB.Driver.Tests.Linq
{
    using System;
    using System.Collections.Generic;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.IdGenerators;
    using MongoDB.Bson.Serialization.Serializers;

    public class MongoDocumentClassSerializer : BsonDocumentBackedClassSerializer<MongoDocument>, IBsonIdProvider
    {
        private readonly List<string> registeredMembers = new List<string>();

        public MongoDocumentClassSerializer()
        {
            this.Register("Id", "_id", new StringSerializer()); 
            this.Register("IsArchived", "IsArchived", new BsonValueSerializer());
        }

        private void Register(string memberName, string elementName, IBsonSerializer serializer)
        {
            registeredMembers.Add(memberName);
            this.RegisterMember(memberName, elementName, serializer);
        }

        public override BsonSerializationInfo GetMemberSerializationInfo(string memberName)
        {
            if (this.registeredMembers.Contains(memberName))
            {
                return base.GetMemberSerializationInfo(memberName);
            }

            return new BsonSerializationInfo(
                    memberName,
                    BsonValueSerializer.Instance,
                    typeof(BsonValue));
        }
        
        protected override MongoDocument CreateInstance(BsonDocument backingDocument)
        {
            return new MongoDocument(backingDocument, this);
        }

        public bool GetDocumentId(object document, out object id, out Type idNominalType, out IIdGenerator idGenerator)
        {
            idNominalType = typeof(string);
            idGenerator = StringObjectIdGenerator.Instance;

            var mongoDocument = document as MongoDocument;
            if (mongoDocument == null)
            {
                id = null;
                return false;
            }

            id = mongoDocument.Id;
            return true;
        }

        public void SetDocumentId(object document, object id)
        {
            var mongoDocument = document as MongoDocument;
            if (mongoDocument != null)
            {
                mongoDocument.Id = id.ToString();
            }
        }
    }
}
