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
using System.Linq;
using System.Reflection;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Events
{
    /// <summary>
    /// Registers methods with a single argument to events 
    /// of that single argument's type.
    /// </summary>
    public class ReflectionEventRegistrar : IEventRegistrar
    {
        private readonly object _instance;
        private readonly string _methodName;
        private readonly BindingFlags _bindingFlags;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionEventRegistrar" /> class.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        public ReflectionEventRegistrar(object instance, string methodName = "Handle", BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
        {
            _instance = Ensure.IsNotNull(instance, "instance");
            _methodName = Ensure.IsNotNullOrEmpty(methodName, "methodName");
            _bindingFlags = bindingFlags;
        }

        /// <summary>
        /// Registers the specified aggregator.
        /// </summary>
        /// <param name="eventAggregator">The aggregator.</param>
        public void Register(IEventAggregator eventAggregator)
        {
            Ensure.IsNotNull(eventAggregator, "eventAggregator");

            var methods = _instance.GetType().GetMethods(_bindingFlags)
                            .Where(x => x.Name == _methodName && x.GetParameters().Length == 1);

            foreach (var method in methods)
            {
                var eventType = method.GetParameters()[0].ParameterType;
                var delegateType = typeof(Action<>).MakeGenericType(eventType);
                var @delegate = method.CreateDelegate(delegateType, _instance);
                eventAggregator.Subscribe(eventType, @delegate);
            }
        }
    }
}
