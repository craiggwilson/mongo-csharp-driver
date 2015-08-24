using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Driver.Linq;
using NUnit.Framework;

namespace MongoDB.Driver.Tests.Linq
{
    [TestFixture]
    public class CompiledQueryTests : IntegrationTestBase
    {
        [Test]
        public void Compiled_query_with_no_arguments()
        {
            var compiled = CompiledQuery.Compile(_collection, q =>
                (from p in q
                 group p by p.A into g
                 select new { g.Key, FirstB = g.First().B }).Any());

            var result = compiled();

            result.Should().BeTrue();
        }

        [Test]
        public void Compiled_query_with_one_argument()
        {
            var compiled = CompiledQuery.Compile<Root, int, IMongoQueryable<Root>>(_collection, (q, i) =>
                q.Where(x => x.C.E.F > i));

            var result = compiled(12);

            result.Count().Should().Be(1);
        }

        [Test]
        public void Compiled_query_with_two_arguments()
        {
            var compiled = CompiledQuery.Compile<Root, int, string, string>(_collection, (q, i, s) =>
                q.Where(x => x.C.E.F > i)
                .Select(x => x.A + s)
                .First());

            var result = compiled(10, "funny");

            result.Should().Be("Awesomefunny");
        }

        [Test]
        public void Timing()
        {

        }
    }
}
