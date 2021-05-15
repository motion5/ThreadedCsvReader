using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThreadedCsvReader.Data.Mappers.TinyCsvParser;
using ThreadedCsvReader.Data.Models;
using ThreadedCsvReader.Util;
using TinyCsvParser;
using TinyCsvParser.Mapping;

namespace ThreadedCsvReader.Parsers
{
    public class TinyCsvParser<T, TM> where TM : CsvMapping<T>, new() where T : class, new()
    {
        private readonly CsvParser<T> parser;

        public TinyCsvParser(CsvParserOptions options = default)
        {
            var parserOptions = options ?? new CsvParserOptions(true, ';');
            var mapper = new TM();
            parser = new CsvParser<T>(parserOptions, mapper);
        }

        private Type GetMapperType()
        {
            var genericInterfaceName = typeof(ICsvMapping<>).Name;

            var types = TypeHelper.GetTypes(x => x.GetInterface(genericInterfaceName) != null && !x.IsAbstract);
            foreach (var type in types)
            {
                if (type.BaseType == GetType().GetGenericTypeDefinition())
                {
                    return type;
                }
            }

            return null;
        }
        
        public IEnumerable<T> RunSequential(string path)
        {
            try
            {
                var result = parser
                    .ReadFromFile(path, Encoding.ASCII).AsSequential();

                return result.Select(r => r.Result);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error parsing csv {exception}");
                throw new Exception($"Unable to parse line of CSV, {exception}");
            }
        }

        public IEnumerable<T> RunParallel(string path)
        {
            try
            {
                var result = parser
                    .ReadFromFile(path, Encoding.ASCII).AsParallel();

                return result.Select(r => r.Result);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error parsing csv {exception}");
                throw new Exception($"Unable to parse line of CSV, {exception}");
            }
        }
    }
}