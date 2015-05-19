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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Events
{
    internal class EventAggregator : IEventPublisherProvider
    {
        private readonly Dictionary<Type, Delegate> _handlers;

        public EventAggregator()
        {
            _handlers = new Dictionary<Type, Delegate>();
        }

        public void Subscribe(Type type, Delegate handler)
        {
            Ensure.IsNotNull(type, "type");
            Ensure.IsNotNull(handler, "handler");

            Delegate chain;
            if (_handlers.TryGetValue(type, out chain))
            {
                chain = Delegate.Combine(chain, handler);
            }
            else
            {
                chain = handler;
            }

            _handlers[type] = chain;
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            Subscribe(typeof(TEvent), handler);
        }

        public bool TryGetPublisher<TEvent>(out Action<TEvent> publisher)
        {
            Delegate handler;
            if (_handlers.TryGetValue(typeof(TEvent), out handler))
            {
                publisher = (Action<TEvent>)handler;
                return true;
            }

            publisher = null;
            return false;
        }
    }
}