using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using ThreadedCsvReader.Data.Models;
using ThreadedCsvReader.Parsers;
using ThreadedCsvReader.Data.Mappers.TinyCsvParser;

namespace ThreadedCsvReader.Benchmark
{
    [ShortRunJob]
    [KeepBenchmarkFiles]
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [RankColumn]
    [HtmlExporter]
    [CsvExporter(CsvSeparator.Comma)]
    [MarkdownExporterAttribute.GitHub]
    public class ParallelCsvRead
    {
        private TinyCsvParser<SalesRecords, SalesRecordsMapping> tinyCsvParser;
        private ParallelParser<SalesRecords, SalesRecordsMapping> parallelParser;
        private ParallelParserV2<SalesRecords, SalesRecordsMapping> parallelParserV2;

        [GlobalSetup]
        public void Setup()
        {
            tinyCsvParser = new TinyCsvParser<SalesRecords, SalesRecordsMapping>();
            parallelParser = new ParallelParser<SalesRecords, SalesRecordsMapping>();
            parallelParserV2 = new ParallelParserV2<SalesRecords, SalesRecordsMapping>();
        }

        public static IEnumerable<(int, int)> ValuesForSemaphore()
        {
            ThreadPool.GetMinThreads(out var minWorkerThreads, out _);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out _);

            for (var i = minWorkerThreads; i < 23; i++)
            {
                yield return (minWorkerThreads, i);
            }
        }
        
        public IEnumerable<SalesRecords> TinyCsvParse500KSalesRecordsParallel() =>
            tinyCsvParser.RunParallel($"{Environment.CurrentDirectory}/100000 Sales Records.csv");

        [Benchmark]
        [ArgumentsSource(nameof(ValuesForSemaphore))]
        public async Task<IEnumerable<SalesRecords>> ParallelParse100KSalesRecords((int minThreads, int maxThreads) threads)
        {
            parallelParser.SemaphoreInit(threads.minThreads, threads.maxThreads);
            return await parallelParser.RunAsync($"{Environment.CurrentDirectory}/100000 Sales Records.csv");
        }
        
        [Benchmark]
        [ArgumentsSource(nameof(ValuesForSemaphore))]
        public Task<IEnumerable<SalesRecords>> ParallelParseV2100KSalesRecords((int minThreads, int maxThreads) threads)
        {
            parallelParserV2.SemaphoreInit(threads.minThreads, threads.maxThreads);
            return parallelParserV2.RunAsync($"{Environment.CurrentDirectory}/100000 Sales Records.csv");
        }
    }
}