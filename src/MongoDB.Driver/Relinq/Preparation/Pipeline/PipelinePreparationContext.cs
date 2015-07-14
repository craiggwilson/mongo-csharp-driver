/* Copyright 2015 MongoDB Inc.
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
using System.Linq.Expressions;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Relinq.Structure.Expressions;
using Remotion.Linq;

namespace MongoDB.Driver.Relinq.Preparation.Pipeline
{
    internal class PipelinePreparationContext : IPipelinePreparationContext
    {
        private readonly ExpressionMapping _mapping;
        private readonly IBsonSerializer _rootSerializer;
        private readonly IBsonSerializerRegistry _serializerRegistry;
        private readonly UniqueIdentifierGenerator _uniqueIdentifierGenerator;

        public PipelinePreparationContext(IBsonSerializer rootSerializer, IBsonSerializerRegistry serializerRegistry, UniqueIdentifierGenerator uniqueIdentifierGenerator)
        {
            _rootSerializer = Ensure.IsNotNull(rootSerializer, "rootSerializer");
            _serializerRegistry = Ensure.IsNotNull(serializerRegistry, "serializerRegistry");
            _uniqueIdentifierGenerator = Ensure.IsNotNull(uniqueIdentifierGenerator, "uniqueIdentifierGenerator");
            _mapping = new ExpressionMapping();
        }

        public void AddExpressionMapping(Expression original, Expression replacement)
        {
            Ensure.IsNotNull(original, "original");
            Ensure.IsNotNull(replacement, "replacement");

            _mapping.AddMapping(original, replacement);
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            return _serializerRegistry.GetSerializer(type);
        }

        public bool TryGetExpressionMapping(Expression original, out Expression replacement)
        {
            return _mapping.TryGetMapping(original, out replacement);
        }

        public Expression PrepareFromExpression(Expression from)
        {
            return from;
        }

        public Expression PrepareSelectExpression(Expression selector)
        {
            Ensure.IsNotNull(selector, "selector");

            selector = PrepareExpression(selector);
            return EnsureSerializationExpression(selector);
        }

        public Expression EnsureSerializationExpression(Expression selector)
        {
            if (!(selector is SerializationExpression))
            {
                var serializer = SerializerBuilder.Build(selector, _serializerRegistry);
                switch (selector.NodeType)
                {
                    case ExpressionType.MemberInit:
                    case ExpressionType.New:
                        return new DocumentExpression(serializer);
                    default:
                        var fieldName = _uniqueIdentifierGenerator.GetUniqueIdentifier("__fld");
                        return WrapField(selector, fieldName, serializer);
                }
            }

            return selector;
        }

        public DocumentWrappedFieldExpression WrapField(Expression expression, string fieldName, IBsonSerializer serializer = null)
        {
            if (serializer == null)
            {
                serializer = SerializerBuilder.Build(expression, _serializerRegistry);
            }

            var wrappingSerializerType = typeof(DocumentWrappedFieldDeserializer<>)
                .MakeGenericType(serializer.ValueType);
            serializer = (IBsonSerializer)Activator.CreateInstance(
                wrappingSerializerType,
                fieldName,
                serializer);

            return new DocumentWrappedFieldExpression(
                expression,
                fieldName,
                serializer);
        }

        public Expression PrepareWhereExpression(Expression predicate)
        {
            Ensure.IsNotNull(predicate, "predicate");
            return PrepareExpression(predicate);
        }

        private Expression PrepareExpression(Expression expression)
        {
            return PreparingExpressionVisitor.Prepare(expression, this);
        }

        private class DocumentWrappedFieldDeserializer<T> : SerializerBase<T>, IBsonDocumentSerializer, IBsonArraySerializer
        {
            private readonly string _fieldName;
            private readonly IBsonSerializer<T> _fieldDeserializer;

            public DocumentWrappedFieldDeserializer(string fieldName, IBsonSerializer<T> fieldDeserializer)
            {
                _fieldName = Ensure.IsNotNull(fieldName, "fieldName");
                _fieldDeserializer = Ensure.IsNotNull(fieldDeserializer, "fieldDeserializer");
            }

            public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                T value = default(T);
                var reader = context.Reader;
                reader.ReadStartDocument();
                while (reader.ReadBsonType() != 0)
                {
                    var fieldName = reader.ReadName();
                    if (fieldName == _fieldName)
                    {
                        value = _fieldDeserializer.Deserialize(context);
                    }
                    else
                    {
                        reader.SkipValue();
                    }
                }
                reader.ReadEndDocument();

                return value;
            }

            public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
            {
                var wrapped = _fieldDeserializer as IBsonDocumentSerializer;
                if (wrapped != null)
                {
                    return wrapped.TryGetMemberSerializationInfo(memberName, out serializationInfo);
                }

                serializationInfo = null;
                return false;
            }

            public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
            {
                var wrapped = _fieldDeserializer as IBsonArraySerializer;
                if (wrapped != null)
                {
                    return wrapped.TryGetItemSerializationInfo(out serializationInfo);
                }

                serializationInfo = null;
                return false;
            }
        }
    }
}
