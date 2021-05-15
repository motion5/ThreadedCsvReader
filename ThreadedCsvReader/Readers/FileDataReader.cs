using System;
using System.IO;

namespace ThreadedCsvReader.Readers
{
    class FileDataReader : DataReader
    {
        public StreamReader Read(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    return new StreamReader(path);
                }

                throw new FileNotFoundException($"File with path {path} not found");
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}