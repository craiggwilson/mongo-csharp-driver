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

using System.Linq;
using System.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.PipelineOperationBinders
{
    internal abstract class PipelineOperationBinder : SerializationInfoBinder
    {
        // protected methods
        protected void RegisterProjector(Expression projector)
        {
            switch (projector.NodeType)
            {
                case ExpressionType.New:
                    MapNewExpressionMembers((NewExpression)projector);
                    break;
            }
        }

        // private methods
        private void MapNewExpressionMembers(NewExpression node)
        {
            // TODO: handle non-anonymous types.  Problems with this would
            // be if they were mapped already, whose serialization information
            // do we use if they conflict (and how do we know if they conflict)?
            if (node.Members != null)
            {
                var properties = node.Type.GetProperties();
                var parameterToPropertyMap = from parameter in node.Constructor.GetParameters()
                                             join property in properties on parameter.Name equals property.Name
                                             select new { Parameter = parameter, Property = property };

                // all of these parameters should be expressions implementing IBsonSerializationInfo
                foreach (var parameterToProperty in parameterToPropertyMap)
                {
                    RegisterMemberReplacement(parameterToProperty.Property, node.Arguments[parameterToProperty.Parameter.Position]);
                }
            }
            else if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(Grouping<,>))
            {
                // All future references to Key will be based on an IGrouping<,> type, not on Grouping<,>.
                var type = typeof(IGrouping<,>).MakeGenericType(node.Type.GetGenericArguments());
                RegisterMemberReplacement(type.GetProperty("Key"), node.Arguments[0]);
            }
        }
    }
}