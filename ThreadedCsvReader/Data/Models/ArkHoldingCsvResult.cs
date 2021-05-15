using System;

namespace ThreadedCsvReader.Data.Models
{
    public abstract class ArkHoldingCsvResult
    {
        public DateTime Date { get; set; }
        public string Fund { get; set; }
        public string Company { get; set; }
        public string Ticker { get; set; }
        public string Cusip { get; set; }
        public decimal Shares { get; set; }
        public decimal MarketValue { get; set; }
        public decimal Weight { get; set; }
    }
}