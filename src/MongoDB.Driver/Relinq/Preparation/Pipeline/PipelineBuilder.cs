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

using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Relinq.Structure;
using MongoDB.Driver.Relinq.Structure.Stages;

namespace MongoDB.Driver.Relinq.Preparation.Pipeline
{
    internal class PipelineBuilder
    {
        private Expression _currentProjector;
        private readonly List<PipelineStage> _stages;

        public PipelineBuilder()
        {
            _stages = new List<PipelineStage>();
        }

        public Expression CurrentProjector
        {
            get { return _currentProjector; }
            set { _currentProjector = value; }
        }

        public IList<PipelineStage> Stages
        {
            get { return _stages; }
        }

        public void AddGroupStage(Expression idSelector)
        {
            _stages.Add(new GroupStage(idSelector));
        }

        public void AddMatchStage(Expression predicate)
        {
            MatchStage previousMatchStage;
            if (TryRemoveLastStage<MatchStage>(out previousMatchStage))
            {
                predicate = Expression.And(previousMatchStage.Predicate, predicate);
            }

            _stages.Add(new MatchStage(predicate));
        }

        public void AddProjectStage(Expression selector)
        {
            _stages.Add(new ProjectStage(selector));
        }

        public PipelineModel Build()
        {
            return new PipelineModel(_stages, _currentProjector);
        }

        public bool TryRemoveLastStage<TStage>(out TStage stage) where TStage : PipelineStage
        {
            if (_stages.Count > 0)
            {
                var lastIndex = _stages.Count - 1;
                stage = _stages[lastIndex] as TStage;
                if (stage != null)
                {
                    _stages.RemoveAt(lastIndex);
                    return true;
                }

                return false;
            }

            stage = null;
            return false;
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
