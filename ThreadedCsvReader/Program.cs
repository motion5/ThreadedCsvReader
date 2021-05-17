using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using CommandLine;
using CsvHelper.Configuration;
using ThreadedCsvReader.Benchmark;
using ThreadedCsvReader.Data.Mappers.TinyCsvParser;
using ThreadedCsvReader.Data.Models;
using ThreadedCsvReader.Parsers;
using TinyCsvParser;
using TinyCsvParser.Mapping;
using TinyCsvParser.Model;

namespace ThreadedCsvReader
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var path = $"{Environment.CurrentDirectory}/5m Sales Records.csv";

            //BenchmarkRunner.Run<SimpleCsvRead>(new DebugInProcessConfig());
            RunTinyCsvParser(path);
            //await RunParallelParser(path);
            
            return;
            var parserOptions = new CsvParserOptions(false, ',');
            var mapper = new SalesRecordsMapping();
            var parser = new CsvParser<SalesRecords>(parserOptions, mapper);

            var csv = File.ReadLines(path);
            var firstLine = csv.First();
            var result = parser
                .Parse(new Row[] {new(0, firstLine)});

            var record = result.First().Result;
            foreach (var propertyInfo in record.GetType().GetProperties())
            {
                Console.Write(record.GetType().GetProperty(propertyInfo.Name)?.GetValue(record, null));
            }

            Console.Read();

        }

        static void RunTinyCsvParser(string path)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var csvReader = new TinyCsvParser<SalesRecords, SalesRecordsMapping>();
            var output = csvReader.RunParallel(path);

            stopwatch.Stop();
            Console.WriteLine($"Parsed {output.Count()} lines in {stopwatch.Elapsed.Seconds} seconds");
        }


        static async Task RunParallelParser(string path)
        {
            ThreadPool.GetMinThreads(out var minWorkerThreads, out var minCompletionPortThreads);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);

            Console.WriteLine("Default min,max number of worker threads for thread pool. Min: {0} Max: {1}",
                minWorkerThreads, maxWorkerThreads);
            Console.WriteLine("Default min,max number of io threads for thread pool. Min: {0} Max: {1}",
                minCompletionPortThreads, maxCompletionPortThreads);

            var stopwatch = Stopwatch.StartNew();
            
            var parallelParser =
                new ParallelParser<SalesRecords, SalesRecordsMapping>();
            parallelParser.SemaphoreInit(minWorkerThreads, 17);
            var output = await parallelParser.RunAsync(path);

            stopwatch.Stop();
            Console.WriteLine($"Parsed {output.Count()} lines in {stopwatch.Elapsed.Seconds} seconds");

            /*
            foreach (var salesRecords in output)
            {
                foreach (var propertyInfo in salesRecords.GetType().GetProperties())
                {
                    Console.Write(salesRecords.GetType().GetProperty(propertyInfo.Name)?.GetValue(salesRecords, null));
                }
            }
            */
        }
    }
}