using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using ThreadedCsvReader.Parsers;

namespace ThreadedCsvReader.Benchmark
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    [RankColumn]
    public class DateParserBenchmark
    {
        const string DateTime = "2099-01-10T16:20:06Z";
        private static readonly DateParser Parser = new();

        [Benchmark(Baseline = true)]
        public void GetYearFromDateTime() => Parser.GetYearFromDateTime(DateTime);

        [Benchmark]
        public void GetYearFromSplit() => Parser.GetYearFromSplit(DateTime);
        
        [Benchmark]
        public void GetYearFromSubstring() => Parser.GetYearFromSubstring(DateTime);
        
        [Benchmark]
        public void GetYearFromSpan() => Parser.GetYearFromSpan(DateTime);
    }
}