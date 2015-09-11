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
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Translators;
using NUnit.Framework;

namespace MongoDB.Driver.Tests.Linq.Translators
{
    [TestFixture]
    public class AggregateProjectTranslatorTests : IntegrationTestBase
    {
        [Test]
        public async Task Should_translate_using_non_anonymous_type_with_default_constructor()
        {
            var result = await Project(x => new RootView { Property = x.A, Field = x.B });

            result.Projection.Should().Be("{ Property: \"$A\", Field: \"$B\", _id: 0 }");

            result.Value.Property.Should().Be("Awesome");
            result.Value.Field.Should().Be("Balloon");
        }

        [Test]
        public async Task Should_translate_using_non_anonymous_type_with_parameterized_constructor()
        {
            var result = await Project(x => new RootView(x.A) { Field = x.B });

            result.Projection.Should().Be("{ Field: \"$B\", Property: \"$A\", _id: 0 }");

            result.Value.Property.Should().Be("Awesome");
            result.Value.Field.Should().Be("Balloon");
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.6")]
        public async Task Should_translate_abs()
        {
            var result = await Project(x => new { Result = Math.Abs(x.C.E.F) });

            result.Projection.Should().Be("{ Result: { \"$abs\": \"$C.E.F\" }, _id: 0 }");

            result.Value.Result.Should().Be(11);
        }

        [Test]
        public async Task Should_translate_add()
        {
            var result = await Project(x => new { Result = x.C.E.F + x.C.E.H });

            result.Projection.Should().Be("{ Result: { \"$add\": [\"$C.E.F\", \"$C.E.H\"] }, _id: 0 }");

            result.Value.Result.Should().Be(33);
        }

        [Test]
        public async Task Should_translate_add_flattened()
        {
            var result = await Project(x => new { Result = x.Id + x.C.E.F + x.C.E.H });

            result.Projection.Should().Be("{ Result: { \"$add\": [\"$_id\", \"$C.E.F\", \"$C.E.H\"] }, _id: 0 }");

            result.Value.Result.Should().Be(43);
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_allElementsTrue()
        {
            var result = await Project(x => new { Result = x.G.All(g => g.E.F > 30) });

            result.Projection.Should().Be("{ Result: { \"$allElementsTrue\" : { \"$map\": { input: \"$G\", as: \"g\", in: { \"$gt\": [\"$$g.E.F\", 30 ] } } } }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_anyElementTrue()
        {
            var result = await Project(x => new { Result = x.G.Any() });

            result.Projection.Should().Be("{ Result: { \"$gt\" : [{ \"$size\" : \"$G\" }, 0] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_anyElementTrue_with_predicate()
        {
            var result = await Project(x => new { Result = x.G.Any(g => g.E.F > 40) });

            result.Projection.Should().Be("{ Result: { \"$anyElementTrue\" : { \"$map\": { input: \"$G\", as: \"g\", in: { \"$gt\": [\"$$g.E.F\", 40 ] } } } }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_anyElementTrue_using_Contains()
        {
            var result = await Project(x => new { Result = x.L.Contains(5) });

            result.Projection.Should().Be("{ Result: { \"$anyElementTrue\": { $map: { input: \"$L\", as: \"x\", in: { $eq: [\"$$x\", 5 ] } } } }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_anyElementTrue_using_Contains_on_a_local_collection()
        {
            var local = new[] { 11, 33, 55 };
            var result = await Project(x => new { Result = local.Contains(x.C.E.F) });

            result.Projection.Should().Be("{ Result: { \"$anyElementTrue\": { $map: { input: [11, 33, 55], as: \"x\", in: { $eq: [\"$$x\", \"$C.E.F\" ] } } } }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        public async Task Should_translate_and()
        {
            var result = await Project(x => new { Result = x.A == "yes" && x.B == "no" });

            result.Projection.Should().Be("{ Result: { \"$and\": [{ \"$eq\": [\"$A\", \"yes\"] }, { \"$eq\": [\"$B\", \"no\"] }] }, _id: 0 }");

            result.Value.Result.Should().BeFalse();
        }

        [Test]
        public async Task Should_translate_and_flattened()
        {
            var result = await Project(x => new { Result = x.A == "yes" && x.B == "no" && x.C.D == "maybe" });

            result.Projection.Should().Be("{ Result: { \"$and\": [{ \"$eq\": [\"$A\", \"yes\"] }, { \"$eq\": [\"$B\", \"no\"] }, { \"$eq\" : [\"$C.D\", \"maybe\"] } ] }, _id: 0 }");

            result.Value.Result.Should().BeFalse();
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_avg()
        {
            var result = await Project(x => new { Result = x.M.Average() });

            result.Projection.Should().Be("{ Result: { \"$avg\": \"$M\" }, _id: 0 }");

            result.Value.Result.Should().BeApproximately(3.66666667, .0001);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_avg_with_selector()
        {
            var result = await Project(x => new { Result = x.G.Average(g => g.E.F) });

            result.Projection.Should().Be("{ Result: { \"$avg\": \"$G.E.F\" }, _id: 0 }");

            result.Value.Result.Should().Be(44);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.6")]
        public async Task Should_translate_ceil()
        {
            var result = await Project(x => new { Result = Math.Ceiling(x.U) });

            result.Projection.Should().Be("{ Result: { \"$ceil\": \"$U\" }, _id: 0 }");

            result.Value.Result.Should().Be(2);
        }

        [Test]
        public async Task Should_translate_coalesce()
        {
            var result = await Project(x => new { Result = x.A ?? "funny" });

            result.Projection.Should().Be("{ Result: { \"$ifNull\": [\"$A\", \"funny\"] }, _id: 0 }");

            result.Value.Result.Should().Be("Awesome");
        }

        [Test]
        public async Task Should_translate_compare()
        {
            var result = await Project(x => new { Result = x.A.CompareTo("Awesome") });

            result.Projection.Should().Be("{ Result: { \"$cmp\": [\"$A\", \"Awesome\"] }, _id: 0 }");

            result.Value.Result.Should().Be(0);
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.4.0")]
        public async Task Should_translate_concat()
        {
            var result = await Project(x => new { Result = x.A + x.B });

            result.Projection.Should().Be("{ Result: { \"$concat\": [\"$A\", \"$B\"] }, _id: 0 }");

            result.Value.Result.Should().Be("AwesomeBalloon");
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.4.0")]
        public async Task Should_translate_concat_flattened()
        {
            var result = await Project(x => new { Result = x.A + " " + x.B });

            result.Projection.Should().Be("{ Result: { \"$concat\": [\"$A\", \" \", \"$B\"] }, _id: 0 }");

            result.Value.Result.Should().Be("Awesome Balloon");
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.6")]
        public async Task Should_translate_concatArrays()
        {
            var result = await Project(x => new { Result = x.M.Concat(x.L) });

            result.Projection.Should().Be("{ Result: { \"$concatArrays\": [\"$M\", \"$L\"] }, _id: 0 }");

            result.Value.Result.Should().BeEquivalentTo(2, 4, 5, 1, 3, 5);
        }

        [Test]
        public async Task Should_translate_condition()
        {
            var result = await Project(x => new { Result = x.A == "funny" ? "a" : "b" });

            result.Projection.Should().Be("{ Result: { \"$cond\": [{ \"$eq\": [\"$A\", \"funny\"] }, \"a\", \"b\"] }, _id: 0 }");

            result.Value.Result.Should().Be("b");
        }

        [Test]
        public async Task Should_translate_day_of_month()
        {
            var result = await Project(x => new { Result = x.J.Day });

            result.Projection.Should().Be("{ Result: { \"$dayOfMonth\": \"$J\" }, _id: 0 }");

            result.Value.Result.Should().Be(1);
        }

        [Test]
        public async Task Should_translate_day_of_week()
        {
            var result = await Project(x => new { Result = x.J.DayOfWeek });

            result.Projection.Should().Be("{ Result: { \"$subtract\" : [{ \"$dayOfWeek\": \"$J\" }, 1] }, _id: 0 }");

            result.Value.Result.Should().Be(DayOfWeek.Saturday);
        }

        [Test]
        public async Task Should_translate_day_of_year()
        {
            var result = await Project(x => new { Result = x.J.DayOfYear });

            result.Projection.Should().Be("{ Result: { \"$dayOfYear\": \"$J\" }, _id: 0 }");

            result.Value.Result.Should().Be(336);
        }

        [Test]
        public async Task Should_translate_divide()
        {
            var result = await Project(x => new { Result = (double)x.C.E.F / x.C.E.H });

            result.Projection.Should().Be("{ Result: { \"$divide\": [\"$C.E.F\", \"$C.E.H\"] }, _id: 0 }");

            result.Value.Result.Should().Be(0.5);
        }

        [Test]
        public async Task Should_translate_divide_3_numbers()
        {
            var result = await Project(x => new { Result = (double)x.Id / x.C.E.F / x.C.E.H });

            result.Projection.Should().Be("{ Result: { \"$divide\": [{ \"$divide\": [\"$_id\", \"$C.E.F\"] }, \"$C.E.H\"] }, _id: 0 }");

            result.Value.Result.Should().BeApproximately(0.04, .01);
        }

        [Test]
        public async Task Should_translate_equals()
        {
            var result = await Project(x => new { Result = x.C.E.F == 5 });

            result.Projection.Should().Be("{ Result: { \"$eq\": [\"$C.E.F\", 5] }, _id: 0 }");

            result.Value.Result.Should().BeFalse();
        }

        [Test]
        public async Task Should_translate_equals_as_a_method_call()
        {
            var result = await Project(x => new { Result = x.C.E.F.Equals(5) });

            result.Projection.Should().Be("{ Result: { \"$eq\": [\"$C.E.F\", 5] }, _id: 0 }");

            result.Value.Result.Should().BeFalse();
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.6")]
        public async Task Should_translate_exp()
        {
            var result = await Project(x => new { Result = Math.Exp(x.C.E.F) });

            result.Projection.Should().Be("{ Result: { \"$exp\": [\"$C.E.F\"] }, _id: 0 }");

            result.Value.Result.Should().BeApproximately(59874.1417151978, .0001);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.6")]
        public async Task Should_translate_floor()
        {
            var result = await Project(x => new { Result = Math.Floor(x.U) });

            result.Projection.Should().Be("{ Result: { \"$floor\": \"$U\" }, _id: 0 }");

            result.Value.Result.Should().Be(1);
        }

        [Test]
        public async Task Should_translate_greater_than()
        {
            var result = await Project(x => new { Result = x.C.E.F > 5 });

            result.Projection.Should().Be("{ Result: { \"$gt\": [\"$C.E.F\", 5] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        public async Task Should_translate_greater_than_or_equal()
        {
            var result = await Project(x => new { Result = x.C.E.F >= 5 });

            result.Projection.Should().Be("{ Result: { \"$gte\": [\"$C.E.F\", 5] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        public async Task Should_translate_hour()
        {
            var result = await Project(x => new { Result = x.J.Hour });

            result.Projection.Should().Be("{ Result: { \"$hour\": \"$J\" }, _id: 0 }");

            result.Value.Result.Should().Be(13);
        }

        [Test]
        public async Task Should_translate_less_than()
        {
            var result = await Project(x => new { Result = x.C.E.F < 5 });

            result.Projection.Should().Be("{ Result: { \"$lt\": [\"$C.E.F\", 5] }, _id: 0 }");

            result.Value.Result.Should().BeFalse();
        }

        [Test]
        public async Task Should_translate_less_than_or_equal()
        {
            var result = await Project(x => new { Result = x.C.E.F <= 5 });

            result.Projection.Should().Be("{ Result: { \"$lte\": [\"$C.E.F\", 5] }, _id: 0 }");

            result.Value.Result.Should().BeFalse();
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_literal_when_a_constant_strings_begins_with_a_dollar()
        {
            var result = await Project(x => new { Result = x.A == "$1" });

            result.Projection.Should().Be("{ Result: { \"$eq\": [\"$A\", { \"$literal\": \"$1\" }] }, _id: 0 }");

            result.Value.Result.Should().BeFalse();
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.6")]
        public async Task Should_translate_ln()
        {
            var result = await Project(x => new { Result = Math.Log(x.C.E.F) });

            result.Projection.Should().Be("{ Result: { \"$ln\": [\"$C.E.F\"] }, _id: 0 }");

            result.Value.Result.Should().BeApproximately(2.39789527279837, .0001);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.6")]
        public async Task Should_translate_log()
        {
            var result = await Project(x => new { Result = Math.Log(x.C.E.F, 11) });

            result.Projection.Should().Be("{ Result: { \"$log\": [\"$C.E.F\", 11.0] }, _id: 0 }");

            result.Value.Result.Should().Be(1);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.6")]
        public async Task Should_translate_log10()
        {
            var result = await Project(x => new { Result = Math.Log10(x.C.E.F) });

            result.Projection.Should().Be("{ Result: { \"$log10\": [\"$C.E.F\"] }, _id: 0 }");

            result.Value.Result.Should().BeApproximately(1.0413928515823, .0001);
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_map_with_document()
        {
            var result = await Project(x => new { Result = x.G.Select(g => g.D + "0") });

            result.Projection.Should().Be("{ Result: { \"$map\": { input: \"$G\", as: \"g\", in: { \"$concat\": [\"$$g.D\", \"0\"] } } }, _id: 0 }");

            result.Value.Result.Should().Equal("Don't0", "Dolphin0");
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_map_with_value()
        {
            var result = await Project(x => new { Result = x.C.E.I.Select(i => i + "0") });

            result.Projection.Should().Be("{ Result: { \"$map\": { input: \"$C.E.I\", as: \"i\", in: { \"$concat\": [\"$$i\", \"0\"] } } }, _id: 0 }");

            result.Value.Result.Should().Equal("it0", "icky0");
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_max()
        {
            var result = await Project(x => new { Result = x.M.Max() });

            result.Projection.Should().Be("{ Result: { \"$max\": \"$M\" }, _id: 0 }");

            result.Value.Result.Should().Be(5);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_max_with_selector()
        {
            var result = await Project(x => new { Result = x.G.Max(g => g.E.F) });

            result.Projection.Should().Be("{ Result: { \"$max\": \"$G.E.F\" }, _id: 0 }");

            result.Value.Result.Should().Be(55);
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.4.0")]
        public async Task Should_translate_millisecond()
        {
            var result = await Project(x => new { Result = x.J.Millisecond });

            result.Projection.Should().Be("{ Result: { \"$millisecond\": \"$J\" }, _id: 0 }");

            result.Value.Result.Should().Be(16);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_min()
        {
            var result = await Project(x => new { Result = x.M.Min() });

            result.Projection.Should().Be("{ Result: { \"$min\": \"$M\" }, _id: 0 }");

            result.Value.Result.Should().Be(2);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_min_with_selector()
        {
            var result = await Project(x => new { Result = x.G.Min(g => g.E.F) });

            result.Projection.Should().Be("{ Result: { \"$min\": \"$G.E.F\" }, _id: 0 }");

            result.Value.Result.Should().Be(33);
        }

        [Test]
        public async Task Should_translate_minute()
        {
            var result = await Project(x => new { Result = x.J.Minute });

            result.Projection.Should().Be("{ Result: { \"$minute\": \"$J\" }, _id: 0 }");

            result.Value.Result.Should().Be(14);
        }

        [Test]
        public async Task Should_translate_modulo()
        {
            var result = await Project(x => new { Result = x.C.E.F % 5 });

            result.Projection.Should().Be("{ Result: { \"$mod\": [\"$C.E.F\", 5] }, _id: 0 }");

            result.Value.Result.Should().Be(1);
        }

        [Test]
        public async Task Should_translate_month()
        {
            var result = await Project(x => new { Result = x.J.Month });

            result.Projection.Should().Be("{ Result: { \"$month\": \"$J\" }, _id: 0 }");

            result.Value.Result.Should().Be(12);
        }

        [Test]
        public async Task Should_translate_multiply()
        {
            var result = await Project(x => new { Result = x.C.E.F * x.C.E.H });

            result.Projection.Should().Be("{ Result: { \"$multiply\": [\"$C.E.F\", \"$C.E.H\"] }, _id: 0 }");

            result.Value.Result.Should().Be(242);
        }

        [Test]
        public async Task Should_translate_multiply_flattened()
        {
            var result = await Project(x => new { Result = x.Id * x.C.E.F * x.C.E.H });

            result.Projection.Should().Be("{ Result: { \"$multiply\": [\"$_id\", \"$C.E.F\", \"$C.E.H\"] }, _id: 0 }");

            result.Value.Result.Should().Be(2420);
        }

        [Test]
        public async Task Should_translate_not()
        {
            var result = await Project(x => new { Result = !x.K });

            result.Projection.Should().Be("{ Result: { \"$not\": [\"$K\"] }, _id: 0 }");

            result.Value.Result.Should().BeFalse();
        }

        [Test]
        public async Task Should_translate_not_with_comparison()
        {
            var result = await Project(x => new { Result = !(x.C.E.F < 3) });

            result.Projection.Should().Be("{ Result: { \"$not\": [{ \"$lt\": [\"$C.E.F\", 3] }] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        public async Task Should_translate_not_equals()
        {
            var result = await Project(x => new { Result = x.C.E.F != 5 });

            result.Projection.Should().Be("{ Result: { \"$ne\": [\"$C.E.F\", 5] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        public async Task Should_translate_or()
        {
            var result = await Project(x => new { Result = x.A == "yes" || x.B == "no" });

            result.Projection.Should().Be("{ Result: { \"$or\": [{ \"$eq\": [\"$A\", \"yes\"] }, { \"$eq\": [\"$B\", \"no\"] }] }, _id: 0 }");

            result.Value.Result.Should().BeFalse();
        }

        [Test]
        public async Task Should_translate_or_flattened()
        {
            var result = await Project(x => new { Result = x.A == "yes" || x.B == "no" || x.C.D == "maybe" });

            result.Projection.Should().Be("{ Result: { \"$or\": [{ \"$eq\": [\"$A\", \"yes\"] }, { \"$eq\": [\"$B\", \"no\"] }, { \"$eq\" : [\"$C.D\", \"maybe\"] } ] }, _id: 0 }");

            result.Value.Result.Should().BeFalse();
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.6")]
        public async Task Should_translate_pow()
        {
            var result = await Project(x => new { Result = Math.Pow(x.C.E.F, 5) });

            result.Projection.Should().Be("{ Result: { \"$pow\": [\"$C.E.F\", 5.0] }, _id: 0 }");

            result.Value.Result.Should().Be(161051);
        }

        [Test]
        public async Task Should_translate_second()
        {
            var result = await Project(x => new { Result = x.J.Second });

            result.Projection.Should().Be("{ Result: { \"$second\": \"$J\" }, _id: 0 }");

            result.Value.Result.Should().Be(15);
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_size_greater_than_zero_from_any()
        {
            var result = await Project(x => new { Result = x.M.Any() });

            result.Projection.Should().Be("{ Result: { \"$gt\": [{ \"$size\": \"$M\" }, 0] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_size_from_an_array()
        {
            var result = await Project(x => new { Result = x.M.Length });

            result.Projection.Should().Be("{ Result: { \"$size\": \"$M\" }, _id: 0 }");

            result.Value.Result.Should().Be(3);
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_size_from_Count_extension_method()
        {
            var result = await Project(x => new { Result = x.M.Count() });

            result.Projection.Should().Be("{ Result: { \"$size\": \"$M\" }, _id: 0 }");

            result.Value.Result.Should().Be(3);
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_size_from_LongCount_extension_method()
        {
            var result = await Project(x => new { Result = x.M.LongCount() });

            result.Projection.Should().Be("{ Result: { \"$size\": \"$M\" }, _id: 0 }");

            result.Value.Result.Should().Be(3);
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_size_from_Count_property_on_Generic_ICollection()
        {
            var result = await Project(x => new { Result = x.L.Count });

            result.Projection.Should().Be("{ Result: { \"$size\": \"$L\" }, _id: 0 }");

            result.Value.Result.Should().Be(3);
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_set_difference()
        {
            var result = await Project(x => new { Result = x.C.E.I.Except(new[] { "it", "not in here" }) });

            result.Projection.Should().Be("{ Result: { \"$setDifference\": [\"$C.E.I\", [\"it\", \"not in here\"] ] }, _id: 0 }");

            result.Value.Result.Should().Equal("icky");
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_set_difference_reversed()
        {
            var result = await Project(x => new { Result = new[] { "it", "not in here" }.Except(x.C.E.I) });

            result.Projection.Should().Be("{ Result: { \"$setDifference\": [[\"it\", \"not in here\"], \"$C.E.I\"] }, _id: 0 }");

            result.Value.Result.Should().Equal("not in here");
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_set_equals()
        {
            var result = await Project(x => new { Result = x.L.SetEquals(new[] { 1, 3, 5 }) });

            result.Projection.Should().Be("{ Result: { \"$setEquals\": [\"$L\", [1, 3, 5]] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_set_equals_reversed()
        {
            var set = new HashSet<int>(new[] { 1, 3, 5 });
            var result = await Project(x => new { Result = set.SetEquals(x.L) });

            result.Projection.Should().Be("{ Result: { \"$setEquals\": [[1, 3, 5], \"$L\"] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_set_intersection()
        {
            var result = await Project(x => new { Result = x.C.E.I.Intersect(new[] { "it", "not in here" }) });

            result.Projection.Should().Be("{ Result: { \"$setIntersection\": [\"$C.E.I\", [\"it\", \"not in here\"] ] }, _id: 0 }");

            result.Value.Result.Should().Equal("it");
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_set_intersection_reversed()
        {
            var result = await Project(x => new { Result = new[] { "it", "not in here" }.Intersect(x.C.E.I) });

            result.Projection.Should().Be("{ Result: { \"$setIntersection\": [[\"it\", \"not in here\"], \"$C.E.I\"] }, _id: 0 }");

            result.Value.Result.Should().Equal("it");
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_set_is_subset()
        {
            var result = await Project(x => new { Result = x.L.IsSubsetOf(new[] { 1, 3, 5 }) });

            result.Projection.Should().Be("{ Result: { \"$setIsSubset\": [\"$L\", [1, 3, 5]] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_set_is_subset_reversed()
        {
            var set = new HashSet<int>(new[] { 1, 3, 5 });
            var result = await Project(x => new { Result = set.IsSubsetOf(x.L) });

            result.Projection.Should().Be("{ Result: { \"$setIsSubset\": [[1, 3, 5], \"$L\"] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_set_union()
        {
            var result = await Project(x => new { Result = x.C.E.I.Union(new[] { "it", "not in here" }) });

            result.Projection.Should().Be("{ Result: { \"$setUnion\": [\"$C.E.I\", [\"it\", \"not in here\"] ] }, _id: 0 }");

            result.Value.Result.Should().BeEquivalentTo("it", "icky", "not in here");
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_set_union_reversed()
        {
            var result = await Project(x => new { Result = new[] { "it", "not in here" }.Union(x.C.E.I) });

            result.Projection.Should().Be("{ Result: { \"$setUnion\": [[\"it\", \"not in here\"], \"$C.E.I\"] }, _id: 0 }");

            result.Value.Result.Should().BeEquivalentTo("it", "icky", "not in here");
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.6")]
        public async Task Should_translate_sqrt()
        {
            var result = await Project(x => new { Result = Math.Sqrt(x.C.E.F) });

            result.Projection.Should().Be("{ Result: { \"$sqrt\": [\"$C.E.F\"] }, _id: 0 }");

            result.Value.Result.Should().BeApproximately(3.31662479, .0001);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.6")]
        public async Task Should_translate_trunc()
        {
            var result = await Project(x => new { Result = Math.Truncate(x.U) });

            result.Projection.Should().Be("{ Result: { \"$trunc\": \"$U\" }, _id: 0 }");

            result.Value.Result.Should().Be(1);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.3")]
        public async Task Should_translate_where_to_filter()
        {
            var result = await Project(x => new { Result = x.G.Where(c => c.E.F == 33) });

            result.Projection.Should().Be("{ Result: { \"$filter\": { \"input\": \"$G\", \"as\": \"c\", \"cond\": { \"$eq\": [\"$$c.E.F\", 33] } } }, _id: 0 }");

            result.Value.Result.Should().HaveCount(1);
            result.Value.Result.Single().D.Should().Be("Don't");
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.3")]
        public async Task Should_translate_where_then_select_to_filter_then_map()
        {
            var result = await Project(x => new { Result = x.G.Where(c => c.E.F == 33).Select(c => c.D) });

            result.Projection.Should().Be("{ Result: { \"$map\": { \"input\": { \"$filter\": { \"input\": \"$G\", \"as\": \"c\", \"cond\": { \"$eq\": [\"$$c.E.F\", 33] } } }, \"as\": \"c\", \"in\": \"$$c.D\" } }, _id: 0 }");

            result.Value.Result.Should().HaveCount(1);
            result.Value.Result.Single().Should().Be("Don't");
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.3")]
        public async Task Should_translate_select_then_where_to_map_then_filter()
        {
            var result = await Project(x => new { Result = x.G.Select(c => c.D).Where(c => c == "Don't") });

            result.Projection.Should().Be("{ Result: { \"$filter\": { \"input\": \"$G.D\", \"as\": \"c\", \"cond\": { \"$eq\": [\"$$c\", \"Don't\"] } } }, _id: 0 }");

            result.Value.Result.Should().HaveCount(1);
            result.Value.Result.Single().Should().Be("Don't");
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.3")]
        public async Task Should_translate_select_with_an_anonymous_type_then_where_to_map_then_filter()
        {
            var result = await Project(x => new { Result = x.G.Select(c => new { c.D, c.E.F }).Where(c => c.F == 33) });

            result.Projection.Should().Be("{ Result: { \"$filter\": { \"input\": { \"$map\" : { \"input\": \"$G\", \"as\": \"c\", \"in\": { \"D\": \"$$c.D\", \"F\": \"$$c.E.F\" } } }, \"as\": \"c\", \"cond\": { \"$eq\": [\"$$c.F\", 33] } } }, _id: 0 }");

            result.Value.Result.Should().HaveCount(1);
            result.Value.Result.Single().D.Should().Be("Don't");
            result.Value.Result.Single().F.Should().Be(33);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_stdDevPop()
        {
            var result = await Project(x => new { Result = x.M.StandardDeviationPopulation() });

            result.Projection.Should().Be("{ Result: { \"$stdDevPop\": \"$M\" }, _id: 0 }");

            result.Value.Result.Should().BeApproximately(1.247219128924647, .0001);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_stdDevPop_with_selector()
        {
            var result = await Project(x => new { Result = x.G.StandardDeviationPopulation(g => g.E.F) });

            result.Projection.Should().Be("{ Result: { \"$stdDevPop\": \"$G.E.F\" }, _id: 0 }");

            result.Value.Result.Should().Be(11);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_stdDevSamp()
        {
            var result = await Project(x => new { Result = x.M.StandardDeviationSample() });

            result.Projection.Should().Be("{ Result: { \"$stdDevSamp\": \"$M\" }, _id: 0 }");

            result.Value.Result.Should().BeApproximately(1.5275252316519468, .0001);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_stdDevSamp_with_selector()
        {
            var result = await Project(x => new { Result = x.G.StandardDeviationSample(g => g.E.F) });

            result.Projection.Should().Be("{ Result: { \"$stdDevSamp\": \"$G.E.F\" }, _id: 0 }");

            result.Value.Result.Should().BeApproximately(15.556349186104045, .0001);
        }

        [Test]
        public async Task Should_translate_string_equals()
        {
            var result = await Project(x => new { Result = x.B.Equals("Balloon") });

            result.Projection.Should().Be("{ Result: { \"$eq\": [\"$B\", \"Balloon\"] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        public async Task Should_translate_string_equals_using_comparison()
        {
            var result = await Project(x => new { Result = x.B.Equals("Balloon", StringComparison.Ordinal) });

            result.Projection.Should().Be("{ Result: { \"$eq\": [\"$B\", \"Balloon\"] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        public async Task Should_translate_string_case_insensitive_equals()
        {
            var result = await Project(x => new { Result = x.B.Equals("balloon", StringComparison.OrdinalIgnoreCase) });

            result.Projection.Should().Be("{ Result: { \"$eq\": [{ \"$strcasecmp\": [\"$B\", \"balloon\"] }, 0] }, _id: 0 }");

            result.Value.Result.Should().BeTrue();
        }

        [Test]
        [TestCase(StringComparison.CurrentCulture)]
        [TestCase(StringComparison.CurrentCultureIgnoreCase)]
        [TestCase(StringComparison.InvariantCulture)]
        [TestCase(StringComparison.InvariantCultureIgnoreCase)]
        public void Should_throw_for_a_not_supported_string_comparison_type(StringComparison comparison)
        {
            Func<Task> act = async () => await Project(x => new { Result = x.B.Equals("balloon", comparison) });

            act.ShouldThrow<NotSupportedException>();
        }

        [Test]
        public async Task Should_translate_string_is_null_or_empty()
        {
            var result = await Project(x => new { Result = string.IsNullOrEmpty(x.B) });

            result.Projection.Should().Be("{ Result: { \"$or\": [{ $eq: [\"$B\", null] }, { $eq: [\"$B\", \"\"] } ] }, _id: 0 }");

            result.Value.Result.Should().BeFalse();
        }

        [Test]
        public async Task Should_translate_substring()
        {
            var result = await Project(x => new { Result = x.B.Substring(3, 20) });

            result.Projection.Should().Be("{ Result: { \"$substr\": [\"$B\",3, 20] }, _id: 0 }");

            result.Value.Result.Should().Be("loon");
        }

        [Test]
        public async Task Should_translate_subtract()
        {
            var result = await Project(x => new { Result = x.C.E.F - x.C.E.H });

            result.Projection.Should().Be("{ Result: { \"$subtract\": [\"$C.E.F\", \"$C.E.H\"] }, _id: 0 }");

            result.Value.Result.Should().Be(-11);
        }

        [Test]
        public async Task Should_translate_subtract_3_numbers()
        {
            var result = await Project(x => new { Result = x.Id - x.C.E.F - x.C.E.H });

            result.Projection.Should().Be("{ Result: { \"$subtract\": [{ \"$subtract\": [\"$_id\", \"$C.E.F\"] }, \"$C.E.H\"] }, _id: 0 }");

            result.Value.Result.Should().Be(-23);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_slice_with_2_arguments()
        {
            var result = await Project(x => new { Result = x.M.Take(2) });

            result.Projection.Should().Be("{ Result: { \"$slice\": [\"$M\", 2] }, _id: 0 }");

            result.Value.Result.Should().BeEquivalentTo(2, 4);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_slice_with_3_arguments()
        {
            var result = await Project(x => new { Result = x.M.Skip(1).Take(2) });

            result.Projection.Should().Be("{ Result: { \"$slice\": [\"$M\", 1, 2] }, _id: 0 }");

            result.Value.Result.Should().BeEquivalentTo(4, 5);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_sum()
        {
            var result = await Project(x => new { Result = x.M.Sum() });

            result.Projection.Should().Be("{ Result: { \"$sum\": \"$M\" }, _id: 0 }");

            result.Value.Result.Should().Be(11);
        }

        [Test]
        [RequiresServer(MinimumVersion = "3.1.7")]
        public async Task Should_translate_sum_with_selector()
        {
            var result = await Project(x => new { Result = x.G.Sum(g => g.E.F) });

            result.Projection.Should().Be("{ Result: { \"$sum\": \"$G.E.F\" }, _id: 0 }");

            result.Value.Result.Should().Be(88);
        }

        [Test]
        public async Task Should_translate_to_lower()
        {
            var result = await Project(x => new { Result = x.B.ToLower() });

            result.Projection.Should().Be("{ Result: { \"$toLower\": \"$B\" }, _id: 0 }");

            result.Value.Result.Should().Be("balloon");
        }

        [Test]
        public async Task Should_translate_to_lower_invariant()
        {
            var result = await Project(x => new { Result = x.B.ToLowerInvariant() });

            result.Projection.Should().Be("{ Result: { \"$toLower\": \"$B\" }, _id: 0 }");

            result.Value.Result.Should().Be("balloon");
        }

        [Test]
        public async Task Should_translate_to_upper()
        {
            var result = await Project(x => new { Result = x.B.ToUpper() });

            result.Projection.Should().Be("{ Result: { \"$toUpper\": \"$B\" }, _id: 0 }");

            result.Value.Result.Should().Be("BALLOON");
        }

        [Test]
        public async Task Should_translate_to_upper_invariant()
        {
            var result = await Project(x => new { Result = x.B.ToUpperInvariant() });

            result.Projection.Should().Be("{ Result: { \"$toUpper\": \"$B\" }, _id: 0 }");

            result.Value.Result.Should().Be("BALLOON");
        }

        [Test]
        public async Task Should_translate_year()
        {
            var result = await Project(x => new { Result = x.J.Year });

            result.Projection.Should().Be("{ Result: { \"$year\": \"$J\" }, _id: 0 }");

            result.Value.Result.Should().Be(2012);
        }

        [Test]
        public async Task Should_translate_array_projection()
        {
            var result = await Project(x => new { Result = x.G.Select(y => y.E.F) });

            result.Projection.Should().Be("{ Result: \"$G.E.F\", _id: 0 }");

            result.Value.Result.Should().BeEquivalentTo(33, 55);
        }

        [Test]
        public async Task Should_translate_a_derived_class_projection()
        {
            var result = await Project(x => new DerivedRootView { Property = x.A, DerivedProperty = x.B });

            result.Projection.Should().Be("{ Property: \"$A\", DerivedProperty: \"$B\", _id: 0 }");

            result.Value.Property.Should().Be("Awesome");
            result.Value.DerivedProperty.Should().Be("Balloon");
        }

        [Test]
        [RequiresServer(MinimumVersion = "2.6.0")]
        public async Task Should_translate_array_projection_complex()
        {
            var result = await Project(x => new { Result = x.G.Select(y => new { y.E.F, y.E.H }) });

            result.Projection.Should().Be("{ Result : { $map: { input: \"$G\", as: \"y\", in: { F : \"$$y.E.F\", H : \"$$y.E.H\" } } }, _id : 0 }");

            result.Value.Result.First().F.Should().Be(33);
            result.Value.Result.First().H.Should().Be(44);
            result.Value.Result.Last().F.Should().Be(55);
            result.Value.Result.Last().H.Should().Be(66);
        }

        private async Task<ProjectedResult<TResult>> Project<TResult>(Expression<Func<Root, TResult>> projector)
        {
            var serializer = BsonSerializer.SerializerRegistry.GetSerializer<Root>();
            var projectionInfo = AggregateProjectTranslator.Translate(projector, serializer, BsonSerializer.SerializerRegistry);

            var pipelineOperator = new BsonDocument("$project", projectionInfo.Document);
            using (var cursor = await _collection.AggregateAsync(new PipelineStagePipelineDefinition<Root, TResult>(new PipelineStageDefinition<Root, TResult>[] { pipelineOperator }, projectionInfo.ProjectionSerializer)))
            {
                var list = await cursor.ToListAsync();
                return new ProjectedResult<TResult>
                {
                    Projection = projectionInfo.Document,
                    Value = (TResult)list[0]
                };
            }
        }

        private class ProjectedResult<T>
        {
            public BsonDocument Projection;
            public T Value;
        }
    }
}
