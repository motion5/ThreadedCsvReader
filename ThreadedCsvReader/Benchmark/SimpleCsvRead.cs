using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using ThreadedCsvReader.Data.Models;
using ThreadedCsvReader.Parsers;

namespace ThreadedCsvReader.Benchmark
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [SimpleJob(RuntimeMoniker.NetCoreApp50)]
    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    [RankColumn]
    [HtmlExporter]
    [CsvExporter(CsvSeparator.Comma)]
    [MarkdownExporterAttribute.GitHub]
    public class SimpleCsvRead
    {
        private CsvParser parser;

        [GlobalSetup]
        public void Setup()
        {
            parser = new CsvParser();
        }

        [Benchmark]
        public IEnumerable<BitcoinTetherCsvRow> ParseBitcoinUsdtCsvRows() => parser.Run();
    }
}