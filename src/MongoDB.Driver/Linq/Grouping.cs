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
using System.Text;

namespace MongoDB.Driver.Linq
{
    /// <summary>
    /// A grouping result from a GroupBy method call.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    internal class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        // private fields
        private readonly TKey _key;
        private readonly IEnumerable<TElement> _elements;

        // constructors
        // MongoDB doesn't do grouping the same way Linq does.  Hence,
        // every "Grouping" from MongoDB consists of a single document.
        /// <summary>
        /// Initializes a new instance of the <see cref="Grouping{TKey,TElement}" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="element">The element.</param>
        public Grouping(TKey key, TElement element)
        {
            _key = key;
            _elements = new[] { element };
        }

        // public properties
        /// <summary>
        /// Gets the key.
        /// </summary>
        public TKey Key
        {
            get { return _key; }
        }

        // public methods
        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        // explicit methods
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _elements.GetEnumerator();
        }
    }
}
