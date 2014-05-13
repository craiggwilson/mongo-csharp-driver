/* Copyright 2010-2014 MongoDB Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors;

namespace MongoDB.Driver.Builders
{
    internal class BuilderHelper
    {
        // private fields
        private readonly SerializationInfoBinder _binder;

        // constructors
        public BuilderHelper(Type type)
        {
            var serializer = BsonSerializer.LookupSerializer(type);
            var info = new BsonSerializationInfo(
                null,
                serializer,
                type,
                serializer.GetDefaultSerializationOptions());

            _binder = new SerializationInfoBinder(info, false);
        }

        public BuilderHelper(BsonSerializationInfo serializationInfo, bool isRootDocumentProjected)
        {
            _binder = new SerializationInfoBinder(serializationInfo, isRootDocumentProjected);
        }

        public BuilderHelper(SerializationInfoBinder binder)
        {
            _binder = binder;
        }

        // public methods
        public Expression EvaluateAndBind(Expression expression)
        {
            var evaluated = PartialEvaluator.Evaluate(expression);
            return _binder.Bind(evaluated);
        }

        public string GetElementName(LambdaExpression memberExpression)
        {
            return GetSerializationInfo(memberExpression).ElementName;
        }

        public IEnumerable<string> GetElementNames(IEnumerable<LambdaExpression> memberExpressions)
        {
            return memberExpressions.Select(x => GetElementName(x));
        }

        public BsonSerializationInfo GetSerializationInfo(LambdaExpression memberExpression)
        {
            var evaluatedMemberExpression = (LambdaExpression)PartialEvaluator.Evaluate(memberExpression);

            var field = _binder.Bind(evaluatedMemberExpression.Body) as FieldExpression;
            if (field == null)
            {
                var message = string.Format("Unabled to identify the serialization information for the expression '{0}'", memberExpression);
                throw new NotSupportedException(message);
            }

            return field.SerializationInfo;
        }

        public BsonSerializationInfo GetItemSerializationInfo(string methodName, BsonSerializationInfo serializationInfo)
        {
            var arraySerializer = serializationInfo.Serializer as IBsonArraySerializer;
            if (arraySerializer != null)
            {
                var itemSerializationInfo = arraySerializer.GetItemSerializationInfo();
                if (itemSerializationInfo != null)
                {
                    IBsonSerializationOptions itemSerializationOptions = null;
                    var arrayOptions = serializationInfo.SerializationOptions as ArraySerializationOptions;
                    if (arrayOptions != null)
                    {
                        itemSerializationOptions = arrayOptions.ItemSerializationOptions;
                        return new BsonSerializationInfo(
                            itemSerializationInfo.ElementName,
                            itemSerializationInfo.Serializer,
                            itemSerializationInfo.NominalType,
                            itemSerializationOptions);
                    }
                    return itemSerializationInfo;
                }
            }

            string message = string.Format("{0} requires that the serializer specified for {1} support items by implementing {2} and returning a non-null result. {3} is the current serializer.",
                methodName,
                serializationInfo.ElementName,
                typeof(IBsonArraySerializer),
                serializationInfo.Serializer.GetType());
            throw new NotSupportedException(message);
        }
    }
}