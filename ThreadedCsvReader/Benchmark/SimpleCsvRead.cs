using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using ThreadedCsvReader.Data.Mappers.CsvParser;
using ThreadedCsvReader.Data.Models;
using ThreadedCsvReader.Parsers;
using ThreadedCsvReader.Data.Mappers.TinyCsvParser;

namespace ThreadedCsvReader.Benchmark
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [RankColumn]
    [HtmlExporter]
    [CsvExporter(CsvSeparator.Comma)]
    [MarkdownExporterAttribute.GitHub]
    public class SimpleCsvRead
    {
        private CsvParser csvParser;
        private TinyCsvParser<SalesRecords, SalesRecordsMapping> tinyCsvParser;

        [GlobalSetup]
        public void Setup()
        {
            csvParser = new CsvParser();
            tinyCsvParser = new TinyCsvParser<SalesRecords, SalesRecordsMapping>();
        }

        public IEnumerable<BitcoinTetherCsvRow> ParseBitcoinUsdtCsvRows() =>
            csvParser.Run<BitcoinTetherCsvRow, BitcoinTetherCsvRowMap>(
                $"{Environment.CurrentDirectory}/Binance_BTCUSDT_1h.csv");

        [Benchmark]
        public IEnumerable<SalesRecords> CsvParse500KSalesRecords() =>
            csvParser.Run<SalesRecords, SalesRecordsRowMap>($"{Environment.CurrentDirectory}/100000 Sales Records.csv");
        
        [Benchmark]
        public IEnumerable<SalesRecords> TinyCsvParse500KSalesRecords() =>
            tinyCsvParser.RunSequential($"{Environment.CurrentDirectory}/100000 Sales Records.csv");
        
        [Benchmark]
        public IEnumerable<SalesRecords> TinyCsvParse500KSalesRecordsParallel() =>
            tinyCsvParser.RunParallel($"{Environment.CurrentDirectory}/100000 Sales Records.csv");
        
        //[Benchmark]
        public IEnumerable<SalesRecords> Parse1500KSalesRecords() =>
            csvParser.Run<SalesRecords, SalesRecordsRowMap>($"{Environment.CurrentDirectory}/1500000 Sales Records.csv");
        
        //[Benchmark]
        public IEnumerable<SalesRecords> Parse5MSalesRecords() =>
            csvParser.Run<SalesRecords, SalesRecordsRowMap>($"{Environment.CurrentDirectory}/5m Sales Records.csv");
    }
}