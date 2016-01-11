using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Benchmarking.Benchmarks;
using Benchmarking.Framework;
using MongoDB.Bson;

namespace Benchmarking
{
    class Program
    {
        private static int _numWarmupIterations = 10;
        private static int _numIterations = 100;
        private static TimeSpan _minTime = TimeSpan.FromMinutes(0);
        private static TimeSpan _maxTime = TimeSpan.FromMinutes(.2);
        private static List<IBenchmarkResultWriter> _writers = new List<IBenchmarkResultWriter>
        {
            new TextBasedBenchmarkResultWriter(Console.Out)
        };

        static void Main(string[] args)
        {
            var datasetsPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "datasets");

            RunBenchmark(new BsonEncodeDocumentBenchmark<BsonDocument>("Flat", Path.Combine(datasetsPath, "flat_bson.json"), 10000));
            RunBenchmark(new BsonEncodeDocumentBenchmark<BsonDocument>("Deep", Path.Combine(datasetsPath, "deep_bson.json"), 10000));
            RunBenchmark(new BsonEncodeDocumentBenchmark<BsonDocument>("Full", Path.Combine(datasetsPath, "full_bson.json"), 10000));

            //RunBenchmark(new BsonDecodeDocumentBenchmark<BsonDocument>("Flat", Path.Combine(datasetsPath, "flat_bson.json"), 10000));
            //RunBenchmark(new BsonDecodeDocumentBenchmark<BsonDocument>("Deep", Path.Combine(datasetsPath, "deep_bson.json"), 10000));
            //RunBenchmark(new BsonDecodeDocumentBenchmark<BsonDocument>("Full", Path.Combine(datasetsPath, "full_bson.json"), 10000));

            //RunBenchmark(new RunCommandBenchmark(10000));

            //RunBenchmark(new FindOneBenchmark(Path.Combine(datasetsPath, "TWEET.json"), 10000));
            //RunBenchmark(new InsertOneBenchmark("Small", Path.Combine(datasetsPath, "SMALL_DOC.json"), 10000));
            //RunBenchmark(new InsertOneBenchmark("Large", Path.Combine(datasetsPath, "LARGE_DOC.json"), 10));

            //RunBenchmark(new FindManyBenchmark(Path.Combine(datasetsPath, "TWEET.json"), 10000));
            //RunBenchmark(new InsertManyBenchmark("Small", Path.Combine(datasetsPath, "SMALL_DOC.json"), 10000));
            //RunBenchmark(new InsertManyBenchmark("Large", Path.Combine(datasetsPath, "LARGE_DOC.json"), 10));

            //RunBenchmark(new GridFSUploadBenchmark(Path.Combine(datasetsPath, "GRIDFS_LARGE")));
            //RunBenchmark(new GridFSDownloadBenchmark(Path.Combine(datasetsPath, "GRIDFS_LARGE")));

            //var ldjsonFiles = Directory.GetFiles(Path.Combine(datasetsPath, "LDJSON_MULTI"));
            //RunBenchmark(new ImportBenchmark(ldjsonFiles));
            //RunBenchmark(new ExportBenchmark(Path.GetTempPath(), ldjsonFiles));

            //var gridFSFiles = Directory.GetFiles(Path.Combine(datasetsPath, "GRIDFS_MULTI"));
            //RunBenchmark(new GridFSImportBenchmark(gridFSFiles));
            //RunBenchmark(new GridFSExportBenchmark(Path.GetTempPath(), gridFSFiles));
        }

        private static void RunBenchmark(Benchmark benchmark)
        {
            var result = new BenchmarkRunner(
                benchmark,
                _numWarmupIterations,
                _numIterations,
                _minTime,
                _maxTime).Run();

            foreach (var writer in _writers)
            {
                writer.Write(result);
            }
        }
    }
}
