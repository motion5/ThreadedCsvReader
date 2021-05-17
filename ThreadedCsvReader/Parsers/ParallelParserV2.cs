using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using TinyCsvParser;
using TinyCsvParser.Mapping;

namespace ThreadedCsvReader.Parsers
{
    public class ParallelParserV2<T, TM> where TM : ICsvMapping<T>, new()
    {
        private SemaphoreSlim semaphore;
        private CsvParser<T> parser;
        private CsvReaderOptions readerOptions;
        private int MinThreads { get; set; }
        private int MaxThreads { get; set; }
        private bool IsDebug { get; }

        private ConcurrentBag<T> Parsed { get; } = new();

        public ParallelParserV2(bool debug = false)
        {
            IsDebug = debug;

            ThreadPool.GetMinThreads(out var minWorkerThreads, out _);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out _);
            SemaphoreInit(minWorkerThreads, maxWorkerThreads);
            ParserInit();
        }

        public async Task<IEnumerable<T>> RunAsync(string path)
        {
            // read csv
            var lines = File.ReadLines(path);

            var lineNumber = 0;
            foreach (var line in lines)
            {
                ThreadPool.QueueUserWorkItem(ThreadProc, (line, lineNumber++));
                if (semaphore.CurrentCount < MaxThreads)
                {
                    await semaphore.WaitAsync();
                }
            }

            // wait until the semaphore finishes
            semaphore.AvailableWaitHandle.WaitOne();
            return Parsed;
        }


        private void ThreadProc(object stateInfo)
        {
            var (data, count) = (ValueTuple<string, int>) stateInfo;
            IEnumerable<CsvMappingResult<T>> result = null;

            try
            {
                result = parser.ReadFromString(readerOptions, data).AsParallel();
                var record = result.First().Result;
                Parsed.Add(record);
            }
            catch (Exception e)
            {
                var error = result.First().Error;
                LogInfo($"Unable to parse line of CSV, {count}. Csv Error {error}. Exception {e}");
            }

            semaphore.Release();
        }


        public void SemaphoreInit(int min, int max)
        {
            (MinThreads, MaxThreads) = (min, max);
            semaphore = new SemaphoreSlim(min, max);
        }

        private void ParserInit()
        {
            var parserOptions = new CsvParserOptions(false, ',');
            var mapper = new TM();
            parser = new CsvParser<T>(parserOptions, mapper);
            readerOptions = new CsvReaderOptions(new[] {NewLine.Environment.ToString()});
        }

        private void LogInfo(string s)
        {
            if (IsDebug)
            {
                Console.WriteLine(s);
            }
        }
    }
}