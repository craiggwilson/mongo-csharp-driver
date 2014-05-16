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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Execution model that represents a query executed with the query engine.
    /// </summary>
    public sealed class QueryModel : ExecutionModel
    {
        // private fields
        private Type _countType;
        private BsonSerializationInfo _distinctValueSerializationInfo;
        private IMongoFields _fields;
        private bool _isDistinct;
        private bool _isCount;
        private int? _numberToSkip;
        private int? _numberToLimit;
        private IMongoQuery _query;
        private IMongoSortBy _sortBy;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryModel" /> class.
        /// </summary>
        public QueryModel()
            : base(ExecutionModelType.Query)
        {
        }

        // public properties
        /// <summary>
        /// Gets or sets the type of the count.  This will be either int or long.
        /// </summary>
        public Type CountType
        {
            get { return _countType; }
            set { _countType = value; }
        }

        /// <summary>
        /// Gets or sets the distinct value serialization info.
        /// </summary>
        public BsonSerializationInfo DistinctValueSerializationInfo
        {
            get { return _distinctValueSerializationInfo; }
            set { _distinctValueSerializationInfo = value; }
        }

        /// <summary>
        /// Gets or sets the fields document.
        /// </summary>
        public IMongoFields Fields
        {
            get { return _fields; }
            set { _fields = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this query is a Count query.
        /// </summary>
        public bool IsCount
        {
            get { return _isCount; }
            set { _isCount = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is a Distinct query.  This implies
        /// that a single field has been identified for selection.
        /// </summary>
        public bool IsDistinct
        {
            get { return _isDistinct; }
            set { _isDistinct = value; }
        }

        /// <summary>
        /// Gets or sets the number to limit.
        /// </summary>
        public int? NumberToLimit
        {
            get { return _numberToLimit; }
            set { _numberToLimit = value; }
        }

        /// <summary>
        /// Gets or sets the number to skip.
        /// </summary>
        public int? NumberToSkip
        {
            get { return _numberToSkip; }
            set { _numberToSkip = value; }
        }

        /// <summary>
        /// Gets or sets the query document.
        /// </summary>
        public IMongoQuery Query
        {
            get { return _query; }
            set { _query = value; }
        }

        /// <summary>
        /// Gets or sets the sort by document.
        /// </summary>
        public IMongoSortBy SortBy
        {
            get { return _sortBy; }
            set { _sortBy = value; }
        }

        // public methods
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (_isDistinct)
            {
                sb.AppendFormat("distinct(\"{0}\"", _distinctValueSerializationInfo.ElementName);
                if (_query != null)
                {
                    sb.Append(_query);
                }
                sb.Append(")");
            }
            else if (_isCount && _numberToLimit == null && _numberToSkip == null)
            {
                sb.Append("count(");
                if (_query != null)
                {
                    sb.Append(_query);
                }
                sb.Append(")");
            }
            else
            {
                if (_query == null)
                {
                    sb.Append("find(");
                }
                else
                {
                    sb.AppendFormat("find({0}", _query);
                }

                if (_fields != null)
                {
                    sb.AppendFormat(", {0})", _fields);
                }
                else
                {
                    sb.Append(")");
                }

                if (_sortBy != null)
                {
                    sb.AppendFormat(".sort({0})", _sortBy);
                }

                if (_numberToSkip.HasValue)
                {
                    sb.AppendFormat(".skip({0})", _numberToSkip);
                }

                if (_numberToLimit.HasValue)
                {
                    sb.AppendFormat(".limit({0})", _numberToLimit);
                }

                if (_isCount)
                {
                    sb.Append(".size()");
                }
            }

            return sb.ToString();
        }

        // internal methods
        internal override object Execute(MongoCollection collection)
        {
            if (_isDistinct)
            {
                return ExecuteDistinct(collection);
            }
            else if (_numberToLimit == null && _numberToSkip == null && _isCount)
            {
                return ExecuteSimpleCount(collection);
            }

            MongoCursor cursor;
            if (Projection.Projector.Parameters[0].Type == typeof(IProjectionValueStore))
            {
                //throw new NotImplementedException();
                var serializer = new ProjectionValueStoreDeserializer(Projection.FieldSerializationInfo);
                cursor = MongoCursor.Create(typeof(IProjectionValueStore), collection, _query, collection.Settings.ReadPreference, serializer);
                cursor.SetFields(_fields);
            }
            else
            {
                var serializer = BsonSerializer.LookupSerializer(DocumentType);
                cursor = MongoCursor.Create(DocumentType, collection, _query, collection.Settings.ReadPreference, serializer);
            }

            if (_numberToSkip != null)
            {
                cursor = cursor.SetSkip(_numberToSkip.Value);
            }
            if (_numberToLimit != null)
            {
                cursor = cursor.SetLimit(_numberToLimit.Value);
            }
            if (_sortBy != null)
            {
                cursor = cursor.SetSortOrder(_sortBy);
            }

            LambdaExpression executor;
            if (_isCount)
            {
                // we ignore projectors here, although there shouldn't be any at all.
                // the only way we get to a Count is if it is the root.
                var cursorParameter = Expression.Parameter(typeof(MongoCursor), "cursor");
                Expression body = Expression.Convert(
                    Expression.Call(
                        cursorParameter,
                        "Size",
                        Type.EmptyTypes),
                    _countType);

                if (Aggregator == null)
                {
                    // it means the caller is expecting an enumerable result, so we'll
                    // change this thing into one...
                    body = Expression.NewArrayInit(_countType, body);
                }

                executor = Expression.Lambda(
                    Expression.Convert(
                        body,
                        typeof(object)),
                    cursorParameter);
            }
            else
            {
                executor = CreateExecutor();
            }

            try
            {
                return executor.Compile().DynamicInvoke(cursor);
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                // a reflection exception occurs in this case 
                // if a database result doesn't conform to a linq to objects
                // result.  For instance, if Max() is used, but no results are returned
                // we would get this exception with an inner exception relating to 
                // the actual exception coming from a .Single() operation.  We
                // want to throw that inner exception, not this one.
                if (tie.InnerException != null)
                {
                    throw tie.InnerException;
                }

                throw;
            }
        }

        // private methods
        private object ExecuteDistinct(MongoCollection collection)
        {
            var command = new CommandDocument
            {
                { "distinct", collection.Name },
                { "key", _distinctValueSerializationInfo.ElementName },
                { "query", BsonDocumentWrapper.Create(_query), _query != null } // query is optional
            };

            var serializerType = typeof(DistinctCommandResultSerializer<>).MakeGenericType(_distinctValueSerializationInfo.NominalType);
            var serializer = (IBsonSerializer)Activator.CreateInstance(serializerType, _distinctValueSerializationInfo.Serializer);

            // TODO: currently no way to hook into collection.Distinct with a custom serializer,
            // so we do this manually.
            var cursor = MongoCursor.Create(
                typeof(DistinctCommandResult<>).MakeGenericType(_distinctValueSerializationInfo.NominalType),
                collection.Database.GetCollection("$cmd"), 
                command, 
                collection.Settings.ReadPreference, 
                serializer);

            cursor.SetLimit(1); // maybe should be -1?

            var result = cursor.OfType<CommandResult>().First();

            if (!result.Ok)
            {
                throw new MongoLinqException(string.Format("Aggregate failed. {0}", result.ErrorMessage));
            }

            return result.GetType().GetProperty("Values").GetValue(result, null);
        }

        private object ExecuteSimpleCount(MongoCollection collection)
        {
            object result;
            if (_query == null)
            {
                result = collection.Count();
            }
            else
            {
                result = collection.Count(_query);
            }

            if (_countType == typeof(int))
            {
                result = Convert.ToInt32(result);
            }

            if (Aggregator == null)
            {
                // Without an aggregator, it means the caller is expecting
                // an enumerable result, not a single count...
                var arr = Array.CreateInstance(result.GetType(), 1);
                arr.SetValue(result, 0);
                result = arr;
            }

            return result;
        }
    }
}