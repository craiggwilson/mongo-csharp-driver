/* Copyright 2010-2015 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using FluentAssertions;
using NUnit.Framework;
using MongoDB.Driver.Core;
using Remotion.Linq.Parsing.Structure;
using MongoDB.Driver.Relinq;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Tests.Relinq
{
    [TestFixture]
    public class RelinqQueryableTests : MongoDB.Driver.Tests.Linq.IntegrationTestBase
    {
        [Test]
        public void Group_method_with_just_id()
        {
            var query = CreateQuery()
                .GroupBy(x => x.A);

            Assert(query,
                2,
                "{ $group: { _id: '$A', __items: { $push: '$$ROOT' } } }");
        }

        [Test]
        public void Group_method_with_element_selector()
        {
            var query = CreateQuery()
                .GroupBy(x => x.A, (k, s) => new { A = k, Count = s.Count() });

            Assert(query,
                2,
                "{ $group: { _id: '$A', Count: { $sum: 1 } } }");
        }

        [Test]
        public void Group_method_using_select()
        {
            var query = CreateQuery()
                .GroupBy(x => x.A)
                .Select(x => new { A = x.Key, Count = x.Count() });

            Assert(query,
                2,
                "{ $group: { _id: '$A', Count: { $sum: 1 } } }");
        }

        [Test]
        public void Select_method_computed_scalar()
        {
            var query = CreateQuery().Select(x => x.A + " " + x.B);

            Assert(query,
                2,
                "{ $project: { __fld0: { $concat: ['$A', ' ', '$B'] }, _id: 0 } }");
        }

        [Test]
        public void Distinct_followed_by_where()
        {
            var query = CreateQuery()
                .Distinct()
                .Where(x => x.A == "Awesome");

            Assert(query,
                1,
                "{ $group: { _id: '$$ROOT' } }",
                "{ $match: { '_id.A': 'Awesome' } }");
        }

        [Test]
        public void Select_method_computed_scalar_followed_by_distinct_followed_by_where()
        {
            var query = CreateQuery()
                .Select(x => x.A + " " + x.B)
                .Distinct()
                .Where(x => x == "Awesome Balloon");

            Assert(query,
                1,
                "{ $group: { _id: { $concat: ['$A', ' ', '$B'] } } }",
                "{ $match: { _id: 'Awesome Balloon' } }");
        }

        [Test]
        public void Select_method_computed_scalar_followed_by_where()
        {
            var query = CreateQuery()
                .Select(x => x.A + " " + x.B)
                .Where(x => x == "Awesome Balloon");

            Assert(query,
                1,
                "{ $project: { __fld0: { $concat: ['$A', ' ', '$B'] }, _id: 0 } }",
                "{ $match: { __fld0: 'Awesome Balloon' } }");
        }

        [Test]
        public void Select_method_with_predicated_any()
        {
            var query = CreateQuery()
                .Select(x => x.G.Any(g => g.D == "Don't"));

            Assert(query,
                2,
                "{ $project: { __fld0: { $anyElementTrue: { $map: { input: '$G', as: 'g', 'in': { $eq: ['$$g.D', \"Don't\"] } } } }, _id: 0 } }");
        }

        [Test]
        public void Where_method()
        {
            var query = CreateQuery()
                .Where(x => x.A == "Awesome");

            Assert(query,
                1,
                "{ $match: { A: 'Awesome' } }");
        }

        [Test]
        public void Where_method_with_predicated_any()
        {
            var query = CreateQuery()
                .Where(x => x.G.Any(g => g.D == "Don't"));

            Assert(query,
                1,
                "{ $match: { 'G.D': \"Don't\" } }");
        }

        private List<T> Assert<T>(IQueryable<T> queryable, int resultCount, params string[] expectedStages)
        {
            var relinqQueryable = (RelinqQueryable<T>)queryable;
            var renderedPipeline = relinqQueryable.RenderPipeline();

            CollectionAssert.AreEqual(expectedStages.Select(x => BsonDocument.Parse(x)).ToList(), renderedPipeline.Documents);

            var results = queryable.ToList();
            results.Count.Should().Be(resultCount);

            return results;
        }

        private IQueryable<Root> CreateQuery()
        {
            var parser = QueryParser.CreateDefault();
            var executor = new RelinqQueryExecutor<Root>(_collection);

            return new RelinqQueryable<Root>(parser, executor);
        }
    }
}
