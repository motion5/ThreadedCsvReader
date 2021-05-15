using System.Diagnostics.Tracing;

namespace ThreadedCsvReader.Events
{
    public static class Keywords
    {
        public const EventKeywords ThreadQueued = (EventKeywords)1;
        public const EventKeywords ThreadStarted = (EventKeywords)2;
        public const EventKeywords ThreadEnded = (EventKeywords)4;
    }
}