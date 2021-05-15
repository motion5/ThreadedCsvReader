using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ThreadedCsvReader.Benchmark;
using ThreadedCsvReader.Parsers;

namespace ThreadedCsvReader
{
    static class Program
    {
        static void Main(string[] args)
        {
            //var csvReader = new CsvParser(args.Length == 1 && args[0] is "--debug");
            //csvReader.Run();
            BenchmarkRunner.Run<SimpleCsvRead>(new DebugInProcessConfig());
        }
    }
}