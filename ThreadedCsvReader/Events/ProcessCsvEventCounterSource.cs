using System.Diagnostics.Tracing;

namespace ThreadedCsvReader.Events
{
    [EventSource(Name = "ProcessCsvEventCounterSource")]
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