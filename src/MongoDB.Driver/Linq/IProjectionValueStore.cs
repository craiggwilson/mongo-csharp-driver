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
using System.Reflection;
using System.Text;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// Represents a value store for a projection operation.
    /// </summary>
    internal interface IProjectionValueStore
    {
        T GetValue<T>(string elementName, object valueIfNotPresent);
    }

    /// <summary>
    /// Holds a single value.
    /// </summary>
    internal class SingleValueProjectionValueStore : IProjectionValueStore
    {
        // private fields
        private object _value;

        // public methods
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="valueIfNotPresent">The value if not present.</param>
        /// <returns></returns>
        public T GetValue<T>(string key, object valueIfNotPresent)
        {
            if (_value == null)
            {
                return (T)valueIfNotPresent;
            }
            return (T)_value;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValue(object value)
        {
            // value will never be an IProjectionValueStore
            _value = value;
        }
    }

    /// <summary>
    /// Holds an array of values.
    /// </summary>
    internal class ArrayProjectionValueStore : IProjectionValueStore
    {
        // private static fields
        private static readonly Dictionary<Type, MethodInfo> __methodCache = new Dictionary<Type, MethodInfo>();

        // private fields
        private readonly List<object> _values;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayProjectionValueStore"/> class.
        /// </summary>
        public ArrayProjectionValueStore()
        {
            _values = new List<object>();
        }

        // public methods
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="valueIfNotPresent">The value if not present.</param>
        /// <returns></returns>
        public T GetValue<T>(string key, object valueIfNotPresent)
        {
            var method = GetGetValuesMethod(typeof(T));

            return (T)(method.Invoke(null, new object[] { key, _values, valueIfNotPresent }));
        }

        /// <summary>
        /// Adds the value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void AddValue(IProjectionValueStore value)
        {
            _values.Add(value);
        }

        // private static methods
        private static MethodInfo GetGetValuesMethod(Type type)
        {
            MethodInfo methodInfo;
            if (!__methodCache.TryGetValue(type, out methodInfo))
            {
                lock (__methodCache)
                {
                    if (!__methodCache.TryGetValue(type, out methodInfo))
                    {
                        var valueType = type.GetGenericArguments()[0];
                        methodInfo = typeof(ArrayProjectionValueStore)
                            .GetMethod("GetValues", BindingFlags.Static | BindingFlags.NonPublic)
                            .MakeGenericMethod(valueType);
                        __methodCache.Add(type, methodInfo);
                    }
                }
            }
            return methodInfo;
        }

        private static IEnumerable<TValue> GetValues<TValue>(string key, List<object> values, object valueIfNotPresent)
        {
            // if key is null, then we want the whole array.  In this case, TValue will be IProjectionValueStore by nature of how
            // the ProjectionDeserializer stores items given what we know about how they will be used.
            if (key == null)
            {
                return values.Cast<TValue>();
            }

            return values.Cast<IProjectionValueStore>().Select(x => x.GetValue<TValue>(key, (TValue)valueIfNotPresent));
        }
    }

    /// <summary>
    /// Holds a map of values.
    /// </summary>
    internal class DocumentProjectionValueStore : IProjectionValueStore
    {
        // private fields
        private readonly Dictionary<string, IProjectionValueStore> _values;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentProjectionValueStore"/> class.
        /// </summary>
        public DocumentProjectionValueStore()
        {
            _values = new Dictionary<string, IProjectionValueStore>();
        }

        // public methods
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <param name="valueIfNotPresent">The value if not present.</param>
        /// <returns></returns>
        public T GetValue<T>(string key, object valueIfNotPresent)
        {
            var valueStoreAndKey = GetValueStore(key, null, valueIfNotPresent);

            if (valueStoreAndKey != null)
            {
                return valueStoreAndKey.ValueStore.GetValue<T>(valueStoreAndKey.RemainingKey, valueIfNotPresent);
            }

            if (valueIfNotPresent is T)
            {
                return (T)valueIfNotPresent;
            }

            return default(T);
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetValue(string key, IProjectionValueStore value)
        {
            _values.Add(key, value);
        }

        // private methods
        private ValueStoreAndKey GetValueStore(string firstKey, string secondKey, object defaultValue)
        {
            IProjectionValueStore valueStore;
            if (_values.TryGetValue(firstKey, out valueStore))
            {
                return new ValueStoreAndKey
                {
                    ValueStore = valueStore,
                    RemainingKey = secondKey
                };
            }

            var lastIndex = firstKey.LastIndexOf('.');
            if (lastIndex == -1)
            {
                return null;
            }

            var move = firstKey.Substring(lastIndex + 1, firstKey.Length - lastIndex - 1);

            firstKey = firstKey.Remove(lastIndex);
            secondKey = BuildKey(secondKey, move);
            return GetValueStore(firstKey, secondKey, defaultValue);
        }

        // private static methods
        private static string BuildKey(string prefix, string addendum)
        {
            if (prefix == null)
            {
                return addendum;
            }

            return prefix + "." + addendum;
        }

        // nested classes
        private class ValueStoreAndKey
        {
            public IProjectionValueStore ValueStore;
            public string RemainingKey;
        }
    }
}