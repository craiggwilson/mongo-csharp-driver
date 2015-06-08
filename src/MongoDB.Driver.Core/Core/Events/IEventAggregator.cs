/* Copyright 2013-2014 MongoDB Inc.
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
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Events
{
    /// <summary>
    /// Aggregates events.
    /// </summary>
    public interface IEventAggregator
    {
        /// <summary>
        /// Subscribes to the specified <paramref name="eventType"/>.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="handler">The handler.</param>
        void Subscribe(Type eventType, Delegate handler);
    }

    /// <summary>
    /// Extensions for <see cref="IEventAggregator"/>.
    /// </summary>
    public static class IEventHandlerRegistryExtensions
    {
        /// <summary>
        /// Subscribes to the specified <typeparamref name="TEvent"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event.</typeparam>
        /// <param name="aggregator">The aggregator.</param>
        /// <param name="handler">The handler.</param>
        public static void Subscribe<TEvent>(this IEventAggregator aggregator, Action<TEvent> handler)
        {
            Ensure.IsNotNull(aggregator, "aggregator");
            aggregator.Subscribe(typeof(TEvent), handler);
        }
    }
}
