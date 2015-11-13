using System;
using System.IO;
using BenchmarkDotNet;
using BenchmarkDotNet.Tasks;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Benchmarking.Bson.Serialization
{
    [BenchmarkTask(platform: BenchmarkPlatform.X86, jitVersion: BenchmarkJitVersion.LegacyJit)]
    //[BenchmarkTask(platform: BenchmarkPlatform.X64, jitVersion: BenchmarkJitVersion.LegacyJit)]
    //[BenchmarkTask(platform: BenchmarkPlatform.X64, jitVersion: BenchmarkJitVersion.RyuJit)]
    public class Deserialize
    {
        private Config _config;

        [Setup]
        public void Setup()
        {
            _config = new Config(ConfigVariant.Small);
        }

        [Benchmark]
        public void Untyped_ByteBufferStream_from_pool()
        {
            using (var buffer = ByteBufferFactory.Create(new InputBufferChunkSource(BsonChunkPool.Default), _config.Bytes.Length))
            {
                buffer.SetBytes(0, _config.Bytes, 0, _config.Bytes.Length);
                using (var stream = new ByteBufferStream(buffer))
                using (var reader = new BsonBinaryReader(stream))
                {
                    var context = BsonDeserializationContext.CreateRoot(reader);
                    _config.DocumentSerializer.Deserialize(context);
                }
            }
        }

        [Benchmark]
        public void Untyped_MemoryStream()
        {
            using (var stream = new MemoryStream(_config.Bytes))
            using (var reader = new BsonBinaryReader(stream))
            {
                var context = BsonDeserializationContext.CreateRoot(reader);
                _config.DocumentSerializer.Deserialize(context);
            }
        }

        [Benchmark]
        public void Typed_ByteBufferStream_from_pool()
        {
            using (var buffer = ByteBufferFactory.Create(new InputBufferChunkSource(BsonChunkPool.Default), _config.Bytes.Length))
            {
                buffer.SetBytes(0, _config.Bytes, 0, _config.Bytes.Length);
                using (var stream = new ByteBufferStream(buffer))
                using (var reader = new BsonBinaryReader(stream))
                {
                    var context = BsonDeserializationContext.CreateRoot(reader);
                    _config.ObjectSerializer.Deserialize(context);
                }
            }
        }

        [Benchmark]
        public void Typed_MemoryStream()
        {
            using (var stream = new MemoryStream(_config.Bytes))
            using (var reader = new BsonBinaryReader(stream))
            {
                var context = BsonDeserializationContext.CreateRoot(reader);
                _config.ObjectSerializer.Deserialize(context);
            }
        }

        [Benchmark]
        public void ConsumeReader()
        {
            using (var stream = new ByteBufferStream(new ByteArrayBuffer(_config.Bytes)))
            using (var reader = new BsonBinaryReader(stream))
            {
                Read(reader);
            }
        }

        private void Read(IBsonReader reader)
        {
            switch (reader.GetCurrentBsonType())
            {
                case MongoDB.Bson.BsonType.Array:
                    ReadArray(reader);
                    break;
                case MongoDB.Bson.BsonType.Binary:
                    reader.ReadBinaryData();
                    break;
                case MongoDB.Bson.BsonType.Boolean:
                    reader.ReadBoolean();
                    break;
                case MongoDB.Bson.BsonType.DateTime:
                    reader.ReadDateTime();
                    break;
                case MongoDB.Bson.BsonType.Document:
                    ReadDocument(reader);
                    break;
                case MongoDB.Bson.BsonType.Double:
                    reader.ReadDouble();
                    break;
                case MongoDB.Bson.BsonType.Int32:
                    reader.ReadInt32();
                    break;
                case MongoDB.Bson.BsonType.Int64:
                    reader.ReadInt64();
                    break;
                case MongoDB.Bson.BsonType.JavaScript:
                    reader.ReadJavaScript();
                    break;
                case MongoDB.Bson.BsonType.JavaScriptWithScope:
                    reader.ReadJavaScriptWithScope();
                    break;
                case MongoDB.Bson.BsonType.MaxKey:
                    reader.ReadMaxKey();
                    break;
                case MongoDB.Bson.BsonType.MinKey:
                    reader.ReadMinKey();
                    break;
                case MongoDB.Bson.BsonType.Null:
                    reader.ReadNull();
                    break;
                case MongoDB.Bson.BsonType.ObjectId:
                    reader.ReadObjectId();
                    break;
                case MongoDB.Bson.BsonType.RegularExpression:
                    reader.ReadRegularExpression();
                    break;
                case MongoDB.Bson.BsonType.String:
                    reader.ReadString();
                    break;
                case MongoDB.Bson.BsonType.Symbol:
                    reader.ReadSymbol();
                    break;
                case MongoDB.Bson.BsonType.Timestamp:
                    reader.ReadTimestamp();
                    break;
                case MongoDB.Bson.BsonType.Undefined:
                    reader.ReadUndefined();
                    break;
                default:
                    throw new Exception($"Unknown bson type: {reader.GetCurrentBsonType()}");
            }
        }

        private void ReadArray(IBsonReader reader)
        {
            reader.ReadStartArray();
            while (reader.ReadBsonType() != MongoDB.Bson.BsonType.EndOfDocument)
            {
                reader.SkipName();
                Read(reader);
            }
            reader.ReadEndArray();
        }

        private void ReadDocument(IBsonReader reader)
        {
            reader.ReadStartDocument();
            while (reader.ReadBsonType() != MongoDB.Bson.BsonType.EndOfDocument)
            {
                reader.ReadName();
                Read(reader);
            }
            reader.ReadEndDocument();
        }
    }
}