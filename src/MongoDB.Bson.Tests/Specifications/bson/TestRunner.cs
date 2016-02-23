/* Copyright 2016 MongoDB Inc.
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
using System.Linq;
using System.Reflection;
using FluentAssertions;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NUnit.Framework;

namespace MongoDB.Bson.Specifications.bson
{
    [TestFixture]
    public class TestRunner
    {
        [TestCaseSource(typeof(TestCaseFactory), "GetTestCases")]
        public void RunTestDefinition(BsonDocument definition)
        {
            var subjectHex = ((string)definition["subject"]).ToLowerInvariant();
            var subjectBytes = BsonUtils.ParseHexString(subjectHex);

            if (definition.Contains("error"))
            {
                using (var stream = new MemoryStream(subjectBytes))
                using (var reader = new BsonBinaryReader(stream))
                {
                    var context = BsonDeserializationContext.CreateRoot(reader);
                    Action act = () => BsonDocumentSerializer.Instance.Deserialize(context);
                    act.ShouldThrow<Exception>();
                }
            }
            else
            {
                BsonDocument subject = null;
                using (var stream = new MemoryStream(subjectBytes))
                using (var reader = new BsonBinaryReader(stream))
                {
                    var context = BsonDeserializationContext.CreateRoot(reader);
                    subject = BsonDocumentSerializer.Instance.Deserialize(context);
                }

                if (!definition.GetValue("decodeOnly", false).ToBoolean())
                {
                    using (var stream = new MemoryStream())
                    using (var writer = new BsonBinaryWriter(stream))
                    {
                        var context = BsonSerializationContext.CreateRoot(writer);
                        BsonDocumentSerializer.Instance.Serialize(context, subject);

                        var actualEncodedHex = BsonUtils.ToHexString(stream.ToArray());
                        actualEncodedHex.Should().Be(subjectHex);
                    }
                }

                if (definition.Contains("extjson"))
                {
                    var extjson = (BsonDocument)definition["extjson"];
                    extjson.Should().Be(subject);
                }

                if (definition.Contains("string"))
                {
                    var value = subject.GetElement(0).Value;
                    value.ToString().Should().Be(definition["string"].ToString());
                }
            }
        }

        private static class TestCaseFactory
        {
            public static IEnumerable<ITestCaseData> GetTestCases()
            {
                const string prefix = "MongoDB.Bson.Tests.Specifications.bson.tests.";
                return Assembly
                    .GetExecutingAssembly()
                    .GetManifestResourceNames()
                    .Where(path => path.StartsWith(prefix) && path.EndsWith(".json"))
                    .SelectMany(path =>
                    {
                        var definition = ReadDefinition(path);
                        var fullName = path.Remove(0, prefix.Length);

                        var dataList = new List<ITestCaseData>();
                        var nameList = new Dictionary<string, int>();
                        foreach (BsonDocument document in (BsonArray)definition["documents"])
                        {
                            var data = new TestCaseData(document);
                            data.Categories.Add("Specifications");
                            data.Categories.Add("bson");

                            var name = GetTestName((string)definition["description"], document);
                            int i = 0;
                            if (nameList.TryGetValue(name, out i))
                            {
                                nameList[name] = i + 1;
                                name += " #" + i;
                            }
                            else
                            {
                                nameList[name] = 1;
                            }

                            dataList.Add(data.SetName(name));
                        }

                        return dataList;
                    });
            }

            private static string GetTestName(string description, BsonDocument definition)
            {
                var name = description;
                if (definition.Contains("description"))
                {
                    name += " - " + (string)definition["description"];
                }
                else if (definition.Contains("error"))
                {
                    name += " - " + (string)definition["error"];
                }

                return name;
            }

            private static BsonDocument ReadDefinition(string path)
            {
                using (var definitionStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
                using (var definitionStringReader = new StreamReader(definitionStream))
                {
                    var definitionString = definitionStringReader.ReadToEnd();
                    return BsonDocument.Parse(definitionString);
                }
            }
        }
    }
}