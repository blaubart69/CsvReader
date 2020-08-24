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
                RunNReco(rdr);
            }
        }
        static void RunNReco(TextReader rdr)
        {
            var csvRdr = new NReco.Csv.CsvReader(rdr, "\t");
            while (csvRdr.Read())
            {
                
            }
        }
    }
}
