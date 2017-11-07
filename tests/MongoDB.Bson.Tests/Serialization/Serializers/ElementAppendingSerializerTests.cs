/* Copyright 2017 MongoDB Inc.
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
using System.IO;
using System.Reflection;
using FluentAssertions;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Moq;
using Xunit;

namespace MongoDB.Bson.Tests.Serialization.Serializers
{
    public class ElementAppendingSerializerTests
    {
        [Fact]
        public void constructor_should_initialize_instance()
        {
            var documentSerializer = BsonDocumentSerializer.Instance;
            var elements = new BsonElement[0];

            var result = new ElementAppendingSerializer<BsonDocument>(documentSerializer, elements);

            result._documentSerializer().Should().BeSameAs(documentSerializer);
            result._elements().Should().Equal(elements);
        }

        [Fact]
        public void constructor_should_enumerate_elements()
        {
            var documentSerializer = BsonDocumentSerializer.Instance;
            var mockElements = new Mock<IEnumerable<BsonElement>>();
            var mockEnumerator = new Mock<IEnumerator<BsonElement>>();
            mockElements.Setup(m => m.GetEnumerator()).Returns(mockEnumerator.Object);
            mockEnumerator.Setup(m => m.MoveNext()).Returns(false);

            var subject = new ElementAppendingSerializer<BsonDocument>(documentSerializer, mockElements.Object);

            mockElements.Verify(m => m.GetEnumerator(), Times.Once);
        }

        [Fact]
        public void constructor_should_throw_when_documentSerializer_is_null()
        {
            var elements = new BsonElement[0];

            var exception = Record.Exception(() => new ElementAppendingSerializer<BsonDocument>(null, elements));

            var e = exception.Should().BeOfType<ArgumentNullException>().Subject;
            e.ParamName.Should().Be("documentSerializer");
        }

        [Fact]
        public void constructor_should_throw_when_elements_is_null()
        {
            var documentSerializer = BsonDocumentSerializer.Instance;

            var exception = Record.Exception(() => new ElementAppendingSerializer<BsonDocument>(documentSerializer, null));

            var e = exception.Should().BeOfType<ArgumentNullException>().Subject;
            e.ParamName.Should().Be("elements");
        }

        [Fact]
        public void ValueType_should_return_expected_result()
        {
            var subject = CreateSubject();

            var result = subject.ValueType;

            result.Should().Be(typeof(BsonDocument));
        }

        [Fact]
        public void Deserialize_should_throw()
        {
            var subject = CreateSubject();
            var reader = new Mock<IBsonReader>().Object;
            var context = BsonDeserializationContext.CreateRoot(reader);
            var args = new BsonDeserializationArgs { NominalType = typeof(BsonDocument) };

            foreach (var useGenericInterface in new[] { false, true })
            {
                Exception exception;
                if (useGenericInterface)
                {
                    exception = Record.Exception(() => subject.Deserialize(context, args));
                }
                else
                {
                    exception = Record.Exception(() => ((IBsonSerializer)subject).Deserialize(context, args));
                }

                exception.Should().BeOfType<NotSupportedException>();
            }
        }

        [Theory]
        [InlineData("{ }", "{ }", "{ }")]
        [InlineData("{ }", "{ a : 1 }", "{ \"a\" : 1 }")]
        [InlineData("{ a : 1 }", "{ }", "{ \"a\" : 1 }")]
        [InlineData("{ a : 1 }", "{ b : 2 }", "{ \"a\" : 1, \"b\" : 2 }")]
        [InlineData("{ a : 1 }", "{ b : 2, c : 3 }", "{ \"a\" : 1, \"b\" : 2, \"c\" : 3 }")]
        [InlineData("{ a : 1 , b : 2 }", "{ }", "{ \"a\" : 1, \"b\" : 2 }")]
        [InlineData("{ a : 1 , b : 2 }", "{ c : 3 }", "{ \"a\" : 1, \"b\" : 2, \"c\" : 3 }")]
        [InlineData("{ a : 1 , b : 2 }", "{ c : 3, d : 4 }", "{ \"a\" : 1, \"b\" : 2, \"c\" : 3, \"d\" : 4 }")]
        public void Serialize_should_have_expected_result(string valueString, string elementsString, string expectedResult)
        {
            var value = BsonDocument.Parse(valueString);
            var elements = BsonDocument.Parse(elementsString).Elements;
            var subject = CreateSubject(elements);

            foreach (var useGenericInterface in new[] { false, true })
            {
                string result;
                using (var textWriter = new StringWriter())
                using (var writer = new JsonWriter(textWriter))
                {
                    var context = BsonSerializationContext.CreateRoot(writer);
                    var args = new BsonSerializationArgs { NominalType = typeof(BsonDocument) };

                    if (useGenericInterface)
                    {
                        subject.Serialize(context, args, value);
                    }
                    else
                    {
                        ((IBsonSerializer)subject).Serialize(context, args, value);
                    }

                    result = textWriter.ToString();
                }

                result.Should().Be(expectedResult);
            }
        }

        [Fact]
        public void Serialize_should_use_standard_GuidRepresentation_for_elements()
        {
            var guid = Guid.Parse("01020304-0506-0708-090a-0b0c0d0e0f10");
            var value = new BsonDocument("a", new BsonBinaryData(guid, GuidRepresentation.Standard));
            var elements = new BsonDocument("b", new BsonBinaryData(guid, GuidRepresentation.Standard));
            var subject = CreateSubject(elements);

            foreach (var useGenericInterface in new[] { false, true })
            {
                string result;
                using (var textWriter = new StringWriter())
                using (var writer = new JsonWriter(textWriter))
                {
                    var context = BsonSerializationContext.CreateRoot(writer);
                    var args = new BsonSerializationArgs { NominalType = typeof(BsonDocument) };

                    if (useGenericInterface)
                    {
                        subject.Serialize(context, args, value);
                    }
                    else
                    {
                        ((IBsonSerializer)subject).Serialize(context, args, value);
                    }

                    result = textWriter.ToString();
                }

                // note that "a" was converted to a CSUUID but "b" was not
                result.Should().Be("{ \"a\" : CSUUID(\"01020304-0506-0708-090a-0b0c0d0e0f10\"), \"b\" : UUID(\"01020304-0506-0708-090a-0b0c0d0e0f10\") }");
            }
        }

        // private methods
        private ElementAppendingSerializer<BsonDocument> CreateSubject(IEnumerable<BsonElement> elements = null)
        {
            var documentSerializer = BsonDocumentSerializer.Instance;
            elements = elements ?? new BsonElement[0];
            return new ElementAppendingSerializer<BsonDocument>(documentSerializer, elements);
        }
    }

    public static class ElementAppendingSerializerReflector
    {
        public static IBsonSerializer<TDocument> _documentSerializer<TDocument>(this ElementAppendingSerializer<TDocument> instance)
        {
            var fieldInfo = typeof(ElementAppendingSerializer<TDocument>).GetField("_documentSerializer", BindingFlags.NonPublic | BindingFlags.Instance);
            return (IBsonSerializer<TDocument>)fieldInfo.GetValue(instance);
        }

        public static List<BsonElement> _elements<TDocument>(this ElementAppendingSerializer<TDocument> instance)
        {
            var fieldInfo = typeof(ElementAppendingSerializer<TDocument>).GetField("_elements", BindingFlags.NonPublic | BindingFlags.Instance);
            return (List<BsonElement>)fieldInfo.GetValue(instance);
        }
    }
}
