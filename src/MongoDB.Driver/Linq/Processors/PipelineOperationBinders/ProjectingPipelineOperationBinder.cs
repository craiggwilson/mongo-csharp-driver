/* Copyright 2010-2012 10gen Inc.
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
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.PipelineOperationBinders
{
    internal abstract class ProjectingPipelineOperationBinder : GroupAwarePipelineOperationBinder
    {
        // constructors
        protected ProjectingPipelineOperationBinder(Dictionary<Expression, GroupExpression> groupMap)
            : base(groupMap)
        { }

        protected IBsonSerializer LookupSerializer(Expression node)
        {
            if (node.NodeType == ExpressionType.New)
            {
                var nex = (NewExpression)node;
                if (nex.Members != null)
                {
                    return BuildSerializerForAnonymousType(nex);
                }
            }

            return BsonSerializer.LookupSerializer(node.Type);
        }

        // protected methods
        private IBsonSerializer BuildSerializerForAnonymousType(NewExpression node)
        {
            // We are building a serializer specifically for an anonymous type based 
            // on serialization information collected from other serializers.
            // We cannot cache this because the compiler reuses the same anonymous type
            // definition in different contexts as long as they are structurally equatable.
            // As such, it might be that two different queries projecting the same shape
            // might need to be deserialized differently.
            var classMapType = typeof(BsonClassMap<>).MakeGenericType(node.Type);
            BsonClassMap classMap = (BsonClassMap)Activator.CreateInstance(classMapType);

            var properties = node.Type.GetProperties();
            var parameterToPropertyMap = from parameter in node.Constructor.GetParameters()
                                         join property in properties on parameter.Name equals property.Name
                                         select new { Parameter = parameter, Property = property };

            foreach (var parameterToProperty in parameterToPropertyMap)
            {
                var argument = node.Arguments[parameterToProperty.Parameter.Position];
                var field = argument as FieldExpression;
                if (field == null)
                {
                    field = new FieldExpression(
                        node.Arguments[parameterToProperty.Parameter.Position],
                        CreateSerializationInfo(parameterToProperty.Property.PropertyType).WithNewName(parameterToProperty.Property.Name),
                        true);
                }

                classMap.MapMember(parameterToProperty.Property)
                    .SetSerializer(field.SerializationInfo.Serializer)
                    .SetElementName(parameterToProperty.Property.Name);
                //TODO: Need to set default value as well...
            }

            // Anonymous types are immutable and have all their values passed in via a ctor.
            classMap.MapConstructor(node.Constructor, properties.Select(x => x.Name).ToArray());
            classMap.Freeze();

            var serializerType = typeof(BsonClassMapSerializer<>).MakeGenericType(node.Type);
            return (IBsonSerializer)Activator.CreateInstance(serializerType, classMap);
        }

        /// <remarks>
        /// This taks a new expression that may include non-field expressions assigned
        /// to properties and turns them into fields assigned to properties.
        /// </remarks>
        protected NewExpression FlattenNewExpression(NewExpression node)
        {
            if (node.Members != null)
            {
                var properties = node.Type.GetProperties();
                var parameterToPropertyMap = from parameter in node.Constructor.GetParameters()
                                             join property in properties on parameter.Name equals property.Name
                                             select new { Parameter = parameter, Property = property };

                var arguments = new List<Expression>();
                foreach (var parameterToProperty in parameterToPropertyMap)
                {
                    var argument = node.Arguments[parameterToProperty.Parameter.Position];
                    var field = argument as FieldExpression;
                    if (field == null)
                    {
                        if (argument.NodeType == ExpressionType.New)
                        {
                            var flattened = FlattenNewExpression((NewExpression)argument);
                            var serializer = LookupSerializer(flattened);
                            field = new FieldExpression(
                                node.Arguments[parameterToProperty.Parameter.Position],
                                new BsonSerializationInfo(
                                    parameterToProperty.Property.Name,
                                    serializer,
                                    parameterToProperty.Property.PropertyType),
                                true);
                        }
                        else
                        {
                            field = new FieldExpression(
                                node.Arguments[parameterToProperty.Parameter.Position],
                                CreateSerializationInfo(parameterToProperty.Property.PropertyType).WithNewName(parameterToProperty.Property.Name),
                                true);
                        }
                    }
                    else if(field.IsProjected)
                    {
                        field = new FieldExpression(
                            field.Expression,
                            field.SerializationInfo.WithNewName(parameterToProperty.Property.Name),
                            true);
                    }
                    arguments.Add(field);
                }

                return Expression.New(node.Constructor, arguments, node.Members);
            }

            return node;
        }

        private BsonSerializationInfo CreateSerializationInfo(Type type)
        {
            var serializer = BsonSerializer.LookupSerializer(type);
            return new BsonSerializationInfo(
                null,
                serializer,
                type);
        }
    }
}