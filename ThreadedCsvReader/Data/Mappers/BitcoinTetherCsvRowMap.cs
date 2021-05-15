using CsvHelper.Configuration;
using ThreadedCsvReader.Data.Models;

namespace ThreadedCsvReader.Data.Mappers
{
    public class BitcoinTetherCsvRowMap : ClassMap<BitcoinTetherCsvRow>
    {
        public BitcoinTetherCsvRowMap()
        {
            Map(x => x.UnixTime).Name("unix");
            Map(x => x.Date).Name("date");
            Map(x => x.Symbol).Name("symbol");
            Map(x => x.Open).Name("open");
            Map(x => x.High).Name("high");
            Map(x => x.Low).Name("low");
            Map(x => x.Close).Name("close");
            Map(x => x.VolumeBtc).Name("Volume BTC");
            Map(x => x.VolumeUsdt).Name("Volume USDT");
            Map(x => x.TradeCount).Name("tradecount");
        }
    }
}