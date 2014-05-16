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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Translators
{
    internal class SortTranslator
    {
        public IMongoSortBy BuildSortBy(ReadOnlyCollection<SortClause> sortClauses)
        {
            var builder = new SortByBuilder();
            var fieldList = new HashSet<string>();
            for (int i = 0; i < sortClauses.Count; i++)
            {
                var field = (FieldExpression)sortClauses[i].Expression;
                var info = field.SerializationInfo;
                var name = info.ElementName;
                if (!fieldList.Contains(name))
                {
                    fieldList.Add(name);
                    if (sortClauses[i].Direction == SortDirection.Ascending)
                    {
                        builder = builder.Ascending(info.ElementName);
                    }
                    else
                    {
                        builder = builder.Descending(info.ElementName);
                    }
                }
            }

            return builder;
        }
    }
}
