using System;
using System.IO;
using System.Text;

namespace RunCsvReader
{
    class Program
    {
        static void Main(string[] args)
        {
            using (TextReader rdr = 
               new StreamReader(
                    new FileStream(@"c:\temp\allc.tsv", FileMode.Open, FileAccess.Read), 
               detectEncodingFromByteOrderMarks: true))
            {
                RunNReco(rdr, ',');
            }
        }
        static void RunNReco(TextReader rdr, char delim)
        {
            var csvRdr = new NReco.Csv.CsvReader(rdr, delimiter: delim.ToString());
            while (csvRdr.Read())
            {
            }
        }
        static void RunV4(TextReader rdr, char delim)
        {
            var csvRdr = new Spi.CsvReader4(rdr, delim);
            while (csvRdr.Read())
            {
            }
        }
    }
}
