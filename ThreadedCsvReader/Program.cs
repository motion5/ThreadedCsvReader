using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadedCsvReader
{
    static class Program
    {
        private static SemaphoreSlim semaphore;

        private const int MinThreads = 8;
        
        private static Lazy<ProcessCsvEventCounterSource> processCsvEventCounterSource =
            new Lazy<ProcessCsvEventCounterSource>(true);

        private static EventCounter requestCounter = new EventCounter("request", eventSource: new ProcessCsvEventCounterSource());
            
        // This thread procedure performs the task.
        static void ThreadProc(object stateInfo)
        {
            Console.WriteLine("Thread Queued");
            processCsvEventCounterSource.Value.ThreadQueued(stateInfo.ToString());
            
            // Each worker thread begins by requesting the semaphore.
            semaphore.Wait();
            
            var stopwatch = Stopwatch.StartNew();
            
            Console.WriteLine("Thread begins, elapsed: {0}", stopwatch.Elapsed.Seconds);
            processCsvEventCounterSource.Value.ThreadStarted(stateInfo.ToString());

            // @TODO: parse csv line
            Thread.Sleep(1000);
            
            var releaseResponse = semaphore.Release();
            // write event
            
            Console.WriteLine("Thread releases the semaphore.");
            Console.WriteLine("Thread {0} previous semaphore count: ", stateInfo);
            processCsvEventCounterSource.Value.ThreadEnded(stateInfo.ToString(), stopwatch.Elapsed.Seconds);
        }

        static async Task Main(string[] args)
        {
            var test = processCsvEventCounterSource.Value;
            
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
            const string uri =
                "https://ark-funds.com/wp-content/fundsiteliterature/csv/ARK_INNOVATION_ETF_ARKK_HOLDINGS.csv";
            var req = (HttpWebRequest) WebRequest.Create(uri);
            var resp = (HttpWebResponse) req.GetResponse();

            using (var reader = new StreamReader(resp.GetResponseStream() ?? throw new Exception()))
            {
                while (!reader.EndOfStream)
                {
                    // read first line of csv
                    var line = await reader.ReadLineAsync();
                    // add to thread pool
                    ThreadPool.QueueUserWorkItem(ThreadProc, line);
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
            Console.ReadLine();
        }
    }
    
    public static class Keywords
    {
        public const EventKeywords ThreadQueued = (EventKeywords)1;
        public const EventKeywords ThreadStarted = (EventKeywords)2;
        public const EventKeywords ThreadEnded = (EventKeywords)4;
    }
    
    //[EventSource(Name = "ProcessCsvEventCounterSource")]
    public sealed class ProcessCsvEventCounterSource : EventSource
    {
        // define the singleton instance of the event source
        private readonly EventCounter requestCounter;

        public ProcessCsvEventCounterSource() : base(EventSourceSettings.Default)
        {
            requestCounter = new EventCounter("request", this);
        }

        /// Call this method to indicate that a request for a URL was made which took a particular amount of time
        [Event(1, Message = "Queued Thread.", Keywords = Keywords.ThreadQueued, Level = EventLevel.Informational)]
        public void ThreadQueued(string payload)
        {
            // Notes:
            //   1. the event ID passed to WriteEvent (1) corresponds to the (implied) event ID
            //      assigned to this method. The event ID could have been explicitly declared
            //      using an EventAttribute for this method
            //   2. Each counter supports a single float value, so conceptually it maps to a single
            //      measurement in the code.
            //   3. You don't have to have log with WriteEvent if you don't think you will ever care about details
            //       of individual requests (that counter data is sufficient).
            WriteEvent(1, payload);    // This logs it to the event stream if events are on.
        }
        
        [Event(2, Message = "Starting Thread", Keywords = Keywords.ThreadStarted, Level = EventLevel.Informational)]
        public void ThreadStarted(string payload)
        {
            WriteEvent(2, payload);    // This logs it to the event stream if events are on.
        }
        
        [Event(3, Message = "Thread execution ended", Keywords = Keywords.ThreadEnded, Level = EventLevel.Informational)]
        public void ThreadEnded(string payload, float elapsedMSec)
        {
            WriteEvent(3, payload, elapsedMSec);    // This logs it to the event stream if events are on.
            requestCounter.WriteMetric(elapsedMSec);        // This adds it to the PerfCounter called 'Request' if PerfCounters are on
        }
    }
}