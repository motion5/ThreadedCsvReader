using System;

namespace ThreadedCsvReader.Parsers
{
    public class DateParser
    {
        public int GetYearFromDateTime(string dateTime) => DateTime.Parse(dateTime).Year;

        public int GetYearFromSplit(string dateTime)
        {
            var splitOnHyphen = dateTime.Split('-');
            return int.Parse(splitOnHyphen[0]);
        }

        public int GetYearFromSubstring(string dateTime)
        {
            var hyphenIndex = dateTime.IndexOf('-');
            return int.Parse(dateTime.Substring(0, hyphenIndex));
        }

        public int GetYearFromSpan(ReadOnlySpan<char> dateTimeAsSpan)
        {
            var hyphenIndex = dateTimeAsSpan.IndexOf("-");
            return int.Parse(dateTimeAsSpan.Slice(0, hyphenIndex));
        }
    }
}