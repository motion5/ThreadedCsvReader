using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using TinyCsvParser;
using TinyCsvParser.Mapping;

namespace ThreadedCsvReader.Parsers
{
    public class ParallelParser<T, TM> where TM : ICsvMapping<T>, new()
    {
        private SemaphoreSlim semaphore;
        private CsvParser<T> parser;
        private CsvReaderOptions readerOptions;
        private int MinThreads { get; set; }
        private int MaxThreads { get; set; }
        private bool IsDebug { get; }

        private ConcurrentBag<T> Parsed { get; } = new();

        public ParallelParser(bool debug = false)
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
            LogInfo($"Lines in file is {lines.Count()}");

            using (var reader = new StreamReader(path))
            {
                var i = 1;
                await reader.ReadLineAsync();
                while (!reader.EndOfStream)
                {
                    // read first line of csv
                    var line = await reader.ReadLineAsync();
                    // add to thread pool
                    ThreadPool.QueueUserWorkItem(ThreadProcV1, (line, i));
                    LogInfo($"Queued work item {++i}");

                    if (semaphore.CurrentCount < MaxThreads)
                    {
                        await semaphore.WaitAsync();
                    }
                }
            }

            // Restore the semaphore count to its maximum value.
            LogInfo("Main thread calls Release() --> ");
            LogInfo($"{semaphore.CurrentCount} tasks can enter the semaphore.");

            // wait until the semaphore finishes
            semaphore.AvailableWaitHandle.WaitOne();
            return Parsed;
        }
        
        public IEnumerable<T> Run(string path)
        {
            using (var reader = new StreamReader(path))
            {
                var i = 1;
                reader.ReadLine();
                while (!reader.EndOfStream)
                {
                    // read first line of csv
                    var line = reader.ReadLine();
                    // add to thread pool
                    ThreadPool.QueueUserWorkItem(ThreadProcV1, (line, i));
                    //LogInfo($"Queued work item {++i}");

                    if (semaphore.CurrentCount < MaxThreads)
                    {
                        semaphore.Wait();
                    }
                }
            }

            // Restore the semaphore count to its maximum value.
            //LogInfo("Main thread calls Release() --> ");
            //LogInfo($"{semaphore.CurrentCount} tasks can enter the semaphore.");

            // wait until the semaphore finishes
            semaphore.AvailableWaitHandle.WaitOne();
            return Parsed;
        }
        
        public IEnumerable<T> RunV2(string path)
        {
            // read csv
            var lines = File.ReadLines(path);

            var lineNumber = 0;
            foreach (var line in lines)
            {
                ThreadPool.QueueUserWorkItem(ThreadProcV1, (line, lineNumber++));
                if (semaphore.CurrentCount < MaxThreads)
                {
                    semaphore.Wait();
                }
            }

            // wait until the semaphore finishes
            semaphore.AvailableWaitHandle.WaitOne();
            return Parsed;
        }


        private void ThreadProcV1(object stateInfo)
        {
            if (stateInfo is ValueTuple<string, int> payload)
            {
                //LogInfo($"Line number {lineCount} Queued");

                IEnumerable<CsvMappingResult<T>> result = null;

                try
                {
                    result = parser.ReadFromString(readerOptions, payload.Item1).AsParallel();
                    var record = result.First().Result;
                    //LogInfo($"Record: {record}");
                    Parsed.Add(record);
                }
                catch (Exception e)
                {
                    var error = result.First().Error;
                    LogInfo($"Unable to parse line of CSV, {payload.Item2}. Csv Error {error}. Exception {e}");
                }
            }

            semaphore.Release();

            //LogInfo("Thread releases the semaphore.");
            //LogInfo($"Thread previous semaphore count: {releaseResponse}");
        }
        
        private void ThreadProcV2(object stateInfo)
        {
            if (stateInfo is ValueTuple<string, int> payload)
            {
                //LogInfo($"Line number {lineCount} Queued");

                IEnumerable<CsvMappingResult<T>> result = null;

                try
                {
                    result = parser.ReadFromString(readerOptions, payload.Item1).AsParallel();
                    var record = result.First().Result;
                    //LogInfo($"Record: {record}");
                    Parsed.Add(record);
                }
                catch (Exception e)
                {
                    var error = result.First().Error;
                    LogInfo($"Unable to parse line of CSV, {payload.Item2}. Csv Error {error}. Exception {e}");
                }
            }

            semaphore.Release();

            //LogInfo("Thread releases the semaphore.");
            //LogInfo($"Thread previous semaphore count: {releaseResponse}");
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