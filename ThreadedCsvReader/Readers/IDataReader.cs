using System.IO;

namespace ThreadedCsvReader.Readers
{
    public interface IDataReader
    {
        public abstract StreamReader Read();
    }
}