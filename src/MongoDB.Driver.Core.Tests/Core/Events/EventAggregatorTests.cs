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
using NUnit.Framework;
using FluentAssertions;

namespace MongoDB.Driver.Core.Events
{
    [TestFixture]
    public class EventAggregatorTests
    {

        [Test]
        public void TryGetPublisher_should_return_false_when_no_subscribers_exist()
        {
            var subject = new EventAggregator();

            Action<int> publisher;
            subject.TryGetPublisher(out publisher).Should().BeFalse();
        }

        [Test]
        public void TryGetPublisher_should_return_true_when_one_subscriber_exists()
        {
            var subject = new EventAggregator();
            subject.Subscribe<int>(x => { });

            Action<int> publisher;
            subject.TryGetPublisher(out publisher).Should().BeTrue();
        }

        [Test]
        public void Publisher_should_invoke_a_single_subscriber()
        {
            var subject = new EventAggregator();
            bool called = false;
            subject.Subscribe<int>(x => called = true);

            Action<int> publisher;
            subject.TryGetPublisher(out publisher);
            publisher(42);

            called.Should().BeTrue();
        }

        [Test]
        public void Publisher_should_invoke_multiple_subscribers()
        {
            var subject = new EventAggregator();
            bool called1 = false;
            bool called2 = false;
            bool called3 = false;
            subject.Subscribe<int>(x => called1 = true);
            subject.Subscribe<int>(x => called2 = true);
            subject.Subscribe<int>(x => called3 = true);

            Action<int> publisher;
            subject.TryGetPublisher(out publisher);
            publisher(42);

            called1.Should().BeTrue();
            called2.Should().BeTrue();
            called3.Should().BeTrue();
        }
    }
}
