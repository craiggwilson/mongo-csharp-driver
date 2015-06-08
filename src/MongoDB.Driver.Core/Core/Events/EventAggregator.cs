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
    internal class EventAggregator : IEventAggregator, IEventSubscriber
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

            Delegate @delegate;
            if (_handlers.TryGetValue(type, out @delegate))
            {
                @delegate = Delegate.Combine(@delegate, handler);
            }
            else
            {
                @delegate = handler;
            }

            _handlers[type] = @delegate;
        }

        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
        {
            Delegate @delegate;
            if (_handlers.TryGetValue(typeof(TEvent), out @delegate))
            {
                handler = (Action<TEvent>)@delegate;
                return true;
            }

            handler = null;
            return false;
        }
    }
}