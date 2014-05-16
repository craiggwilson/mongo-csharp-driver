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
using System.Threading;

namespace MongoDB.Driver.Linq
{
    internal class LeastRecentlyUsedCache<T>
    {
        // private fields
        private readonly Func<T, T, bool> _comparer;
        private readonly ReaderWriterLockSlim _lock;
        private readonly int _maxSize;
        private readonly List<T> _values;
        private int _version;

        // constructors
        public LeastRecentlyUsedCache(int maxSize, Func<T, T, bool> comparer)
        {
            _comparer = comparer;
            _maxSize = maxSize;
            _lock = new ReaderWriterLockSlim();
            _values = new List<T>();
        }

        // public properties
        public int MaxSize
        {
            get { return _maxSize; }
        }

        // public methods
        public T GetOrAdd(T item)
        {
            T cachedItem;
            int cachedIndex;
            int startVersion = Interlocked.CompareExchange(ref _version, 0, 0);

            _lock.EnterReadLock();
            try
            {
                Find(item, out cachedIndex, out cachedItem);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            _lock.EnterWriteLock();
            try
            {
                bool incrementVersion = false;
                if (_version != startVersion)
                {
                    Find(item, out cachedIndex, out cachedItem);
                }

                if (cachedIndex == -1)
                {
                    _values.Insert(0, item);
                    cachedItem = item;
                    cachedIndex = 0;
                    incrementVersion = true;
                }
                else if (cachedIndex > 0)
                {
                    _values.RemoveAt(cachedIndex);
                    _values.Insert(0, item);
                    cachedIndex = 0;
                    incrementVersion = true;
                }

                if (_values.Count > _maxSize)
                {
                    _values.RemoveAt(_maxSize - 1);
                    incrementVersion = true;
                }

                if (incrementVersion)
                {
                    Interlocked.Increment(ref _version);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            return cachedItem;
        }

        // private methods
        private void Find(T item, out int index, out T cachedItem)
        {
            index = -1;
            cachedItem = default(T);

            for (int i = 0; i < _values.Count; i++)
            {
                cachedItem = _values[i];
                if (_comparer(item, cachedItem))
                {
                    index = i;
                    break;
                }
            }
        }
    }
}