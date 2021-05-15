using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ThreadedCsvReader.Benchmark;
using ThreadedCsvReader.Data.Mappers.TinyCsvParser;
using ThreadedCsvReader.Data.Models;
using ThreadedCsvReader.Parsers;

namespace ThreadedCsvReader
{
    static class Program
    {
        static void Main(string[] args)
        {
            //var path = $"{Environment.CurrentDirectory}/Binance_BTCUSDT_1h.csv";
            //var path = $"{Environment.CurrentDirectory}/500000 Sales Records.csv";
            //var csvReader = new TinyCsvParser<SalesRecords, SalesRecordsMapping>();
            //csvReader.RunSequential(path);
            BenchmarkRunner.Run<SimpleCsvRead>(new DebugInProcessConfig());
        }
    }
}