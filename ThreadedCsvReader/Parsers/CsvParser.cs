using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CsvHelper;
using ThreadedCsvReader.Data.Mappers;
using ThreadedCsvReader.Data.Models;
using ThreadedCsvReader.Readers;

namespace ThreadedCsvReader.Parsers
{
    public class CsvParser
    {
        public bool IsDebug;
        
        public CsvParser(bool isDebug = false)
        {
            IsDebug = isDebug;
        }
        
        public IEnumerable<BitcoinTetherCsvRow> Run()
        {
            var bitcoinUsdtCsvPath = $"{Environment.CurrentDirectory}/Binance_BTCUSDT_1h.csv";

            var fileDataReader = new FileDataReader();
            var streamReader = fileDataReader.Read(bitcoinUsdtCsvPath);
            var data = new List<BitcoinTetherCsvRow>();

            // skip first line of file
            try
            {
                using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);
                // if there is only 1 column, then skip (first row has one item)
                //csv.Configuration.PrepareHeaderForMatch = (header, index) => header.ToLower();
                csv.Configuration.RegisterClassMap<BitcoinTetherCsvRowMap>();
                csv.Configuration.Delimiter = ",";
                while (csv.Read())
                {
                    try
                    {
                        var record = csv.GetRecord<BitcoinTetherCsvRow>();
                        if (IsDebug)
                        {
                            LogRecord(record);
                        }
                        data.Add(record);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine($"Unable to parse line of CSV, {exception}");
                        //throw new Exception($"Unable to parse line of CSV, {exception}");
                    }
                }

                return data;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error parsing csv {exception}");
                throw new Exception($"Unable to parse line of CSV, {exception}");
            }
        }

        private void LogRecord(BitcoinTetherCsvRow record)
        {
            var builder = new StringBuilder("Record:");
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)
                .AddMilliseconds(record.UnixTime);

            //date,symbol,open,high,low,close,Volume BTC, Volume USDT,tradecount

            foreach (var property in record.GetType().GetProperties())
            {
                if (property.Name is "UnixTime")
                {
                    builder.Append("DateTime: ");
                    builder.Append(dateTime.ToUniversalTime());
                    builder.Append('\n');
                }
                else
                {
                    builder.Append(property.Name);
                    builder.Append(": ");
                    builder.Append(record.GetType().GetProperty(property.Name)?.GetValue(record, null));
                    builder.Append('\n');
                }
            }

            Console.WriteLine(builder.ToString());
        }
    }
}