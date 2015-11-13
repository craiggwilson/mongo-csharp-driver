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
    public class Serialize
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
            using (var buffer = new MultiChunkBuffer(new OutputBufferChunkSource(BsonChunkPool.Default)))
            using (var stream = new ByteBufferStream(buffer))
            using (var writer = new BsonBinaryWriter(stream))
            {
                var context = BsonSerializationContext.CreateRoot(writer);
                _config.DocumentSerializer.Serialize(context, _config.Document);
            }
        }

        [Benchmark]
        public void Untyped_MemoryStream()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BsonBinaryWriter(stream))
            {
                var context = BsonSerializationContext.CreateRoot(writer);
                _config.DocumentSerializer.Serialize(context, _config.Document);
            }
        }

        [Benchmark]
        public void Typed_ByteBufferStream_from_pool()
        {
            using (var buffer = new MultiChunkBuffer(new OutputBufferChunkSource(BsonChunkPool.Default)))
            using (var stream = new ByteBufferStream(buffer))
            using (var writer = new BsonBinaryWriter(stream))
            {
                var context = BsonSerializationContext.CreateRoot(writer);
                _config.ObjectSerializer.Serialize(context, _config.Object);
            }
        }

        [Benchmark]
        public void Typed_MemoryStream()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BsonBinaryWriter(stream))
            {
                var context = BsonSerializationContext.CreateRoot(writer);
                _config.ObjectSerializer.Serialize(context, _config.Object);
            }
        }
    }
}