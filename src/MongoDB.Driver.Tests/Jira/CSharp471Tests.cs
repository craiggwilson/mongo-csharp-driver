/* Copyright 2010-2014 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Tests.Linq;
using NUnit.Framework;

namespace MongoDB.Driver.Tests.Jira
{
    [TestFixture]
    public class CSharp471Tests : LinqTestBase
    {
        public class Base
        {
            public Guid Id { get; set; }

            public string A { get; set; }
        }

        public class T1 : Base
        {
            public string B { get; set; }
        }

        public class T2 : Base
        {
            public string C { get; set; }
        }

        [Test]
        public void CastTest()
        {
            var db = Configuration.TestDatabase;
            var collection = db.GetCollection<Base>("castTest");
            collection.Drop();

            var t1 = new T1 { Id = Guid.NewGuid(), A = "T1.A", B = "T1.B" };
            var t2 = new T2 { Id = Guid.NewGuid(), A = "T2.A" };
            collection.Insert(t1);
            collection.Insert(t2);

            var query = from t in collection.AsQueryable()
                        where t is T1 && ((T1)t).B == "T1.B" 
                        select t;

            var model = GetQueryModel(query);

            Assert.AreEqual("{ \"_t\" : \"T1\", \"B\" : \"T1.B\" }", model.Query.ToJson());

            var results = query.ToList();
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0], Is.InstanceOf(typeof(T1)));
            Assert.That(results[0].A, Is.EqualTo("T1.A"));
        }
    }
}