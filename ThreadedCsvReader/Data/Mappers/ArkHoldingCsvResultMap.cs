using CsvHelper.Configuration;
using ThreadedCsvReader.Data.Models;

namespace ThreadedCsvReader.Data.Mappers
{
    public sealed class ArkHoldingCsvResultMap : ClassMap<ArkHoldingCsvResult>
    {
        public ArkHoldingCsvResultMap()
        {
            Map(x => x.Date).Name("date");
            Map(x => x.Fund).Name("fund");
            Map(x => x.Company).Name("company");
            Map(x => x.Ticker).Name("ticker");
            Map(x => x.Cusip).Name("cusip");
            Map(x => x.Shares).Name("shares");
            Map(x => x.MarketValue).Name("market value($)");
            Map(x => x.Weight).Name("weight(%)");
        }
    }
}