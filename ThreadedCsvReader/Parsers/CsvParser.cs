using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using ThreadedCsvReader.Readers;

namespace ThreadedCsvReader.Parsers
{
    public class CsvParser
    {
        private readonly bool isDebug;
        
        public CsvParser(bool isDebug = false)
        {
            this.isDebug = isDebug;
        }
        
        public IEnumerable<T> Run<T, TU>(string path) where TU : ClassMap
        {
            var fileDataReader = new FileDataReader();
            var data = new List<T>();
            var streamReader = fileDataReader.Read(path);

            try
            {
                using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);
                csv.Configuration.RegisterClassMap<TU>();
                while (csv.Read())
                {
                    try
                    {
                        var record = csv.GetRecord<T>();
                        if (isDebug)
                        {
                            LogRecord(record);
                        }
                        data.Add(record);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine($"Unable to parse line of CSV, {exception}");
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
        
        private static void LogRecord<T>(T record)
        {
            var builder = new StringBuilder();

            foreach (var property in record.GetType().GetProperties())
            {
                builder.Append(property.Name);
                builder.Append(": ");
                builder.Append(record.GetType().GetProperty(property.Name)?.GetValue(record, null));
                builder.Append('\n');
            }

            Console.WriteLine(builder.ToString());
        }
    }
}