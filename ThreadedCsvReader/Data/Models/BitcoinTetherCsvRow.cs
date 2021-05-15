namespace ThreadedCsvReader.Data.Models
{
    public class BitcoinTetherCsvRow
    {
        public double UnixTime { get; set; }
        public string Date { get; set; }
        public string Symbol { get; set; }
        public string Open { get; set; }
        public string High { get; set; }
        public string Low { get; set; }
        public string Close { get; set; }
        public string VolumeBtc { get; set; }
        public string VolumeUsdt { get; set; }
        public string TradeCount { get; set; }
    }
}