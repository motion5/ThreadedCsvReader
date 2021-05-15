using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using ThreadedCsvReader.Data.Mappers;
using ThreadedCsvReader.Data.Models;
using ThreadedCsvReader.Events;

namespace ThreadedCsvReader
{
    public class Handlers
    {
        private static SemaphoreSlim semaphore;

        private const int MinThreads = 8;
        
        private static readonly Lazy<ProcessCsvEventCounterSource> ProcessCsvEventCounterSource =
            new Lazy<ProcessCsvEventCounterSource>(true);

        private static EventCounter requestCounter = new EventCounter("request", eventSource: new ProcessCsvEventCounterSource());

        private static readonly HttpClient Client = new HttpClient();
            
        // This thread procedure performs the task.
        private static void ThreadProc(object stateInfo)
        {
            var payload = stateInfo as string;
            Console.WriteLine("Thread Queued");
            ProcessCsvEventCounterSource.Value.ThreadQueued(payload);
            
            // Each worker thread begins by requesting the semaphore.
            semaphore.Wait();
            
            var stopwatch = Stopwatch.StartNew();
            
            Console.WriteLine("Thread begins, elapsed: {0}", stopwatch.Elapsed.Seconds);
            ProcessCsvEventCounterSource.Value.ThreadStarted(payload);

            // @TODO: parse csv line
            try
            {
                using var reader = new StringReader(payload ?? throw new InvalidOperationException("Null string passed to parser"));
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                //csv.Configuration.PrepareHeaderForMatch = (header, index) => header.ToLower();
                csv.Configuration.RegisterClassMap<ArkHoldingCsvResultMap>();
                if (csv.Read())
                {
                    var record = csv.GetRecord<ArkHoldingCsvResult>();
                    Console.WriteLine($"Record: {record}");
                }
                else
                {
                    throw new Exception("Couldn't read next line");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error parsing csv {e}");
            }

            var releaseResponse = semaphore.Release();
            
            Console.WriteLine("Thread releases the semaphore.");
            Console.WriteLine("Thread {0} previous semaphore count: ", stateInfo);
            ProcessCsvEventCounterSource.Value.ThreadEnded(payload, stopwatch.Elapsed.Seconds);
        }


        public async Task EntryPoint()
        {
            
            var test = ProcessCsvEventCounterSource.Value;
            
            ThreadPool.GetMinThreads(out var minWorkerThreads, out var minCompletionPortThreads);
            ThreadPool.GetMaxThreads(out var maxWorkerThreads, out var maxCompletionPortThreads);
            
            Console.WriteLine("Default min,max number of worker threads for thread pool. Min: {0} Max: {1}", minWorkerThreads, maxWorkerThreads);
            Console.WriteLine("Default min,max number of io threads for thread pool. Min: {0} Max: {1}", minCompletionPortThreads, maxCompletionPortThreads);
            
            // set minimum threads
            var systemThreads = Process.GetCurrentProcess().Threads;
            ThreadPool.SetMinThreads(systemThreads.Count, 1);
            Console.WriteLine("System threads is {0}", systemThreads.Count);
            
            // Create a semaphore that can satisfy up to three
            // concurrent requests. Use an initial count of zero,
            // so that the entire semaphore count is initially
            // owned by the main program thread.
            //
            
            // We always want at least our minimum number of threads set, if the system has more available then use that
            var currentMin = Math.Max(systemThreads.Count, MinThreads);
            semaphore = new SemaphoreSlim(currentMin, currentMin);
            
            // read csv
            //await using var responseStream = await new WebClient(new HttpClient()).GetCsvStream();
            //using (var reader = new StreamReader(responseStream ?? throw new InvalidOperationException()))
            var filePath = $"{Environment.CurrentDirectory}/Binance_BTCUSDT_1h.csv";
            
            var lines = File.ReadLines(filePath);
            Console.WriteLine($"Lines in file is {lines.Count()}");

            using (var reader = new StreamReader(filePath))
            {
                var i = 0;
                while (!reader.EndOfStream)
                {
                    if (i == 1)
                    {
                        break;
                    }
                    // read first line of csv
                    var line = await reader.ReadLineAsync();
                    // add to thread pool
                    ThreadPool.QueueUserWorkItem(ThreadProc, line);
                    Console.WriteLine($"Queued work item {++i}");
                }

            }
            
            // Restore the semaphore count to its maximum value.
            Console.Write("Main thread calls Release() --> ");
            Console.WriteLine("{0} tasks can enter the semaphore.", semaphore.CurrentCount);

            //csv.Configuration.PrepareHeaderForMatch = (header, index) => header.ToLower();

            // pass off to pipeline

            // string of line

            // pipeline 
            // threadpool 

            // semaphore

            // cpu bound vs io bound threads
            // processing string = cpu bound thread
            // 
            // kick off a whole bumch of threads and then limit the amount of threads that can run
            // the way you limit is with 2 ways, a mutex or a semaphore
            // mutex -> turnstyle 1 in one out (look into use cases) -> good way to stack up threads and run one at a time
            // semaphore -> liike a bouncer at a club, club is a cpu, 4 people allowed in club, clicks 4 people, 1 person leaves, decrements, back up to 4 threads
            // use a semaphore to find limits by incrementing the amount you lett through at one time, by changing param on semaphore
            
            //Console.ReadLine();
        }
 
    }
}