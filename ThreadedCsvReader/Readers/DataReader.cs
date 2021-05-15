using System.IO;

namespace ThreadedCsvReader.Readers
{
    class DataReader : IDataReader
    {
        public virtual StreamReader Read()
        {
            return StreamReader.Null;
        }
    }
}