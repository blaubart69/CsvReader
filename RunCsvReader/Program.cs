using System;
using System.IO;
using System.Text;

namespace RunCsvReader
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var bs = new BufferedStream(new FileStream(@"c:\temp\allc.tsv", FileMode.Open)))
            using (TextReader rdr = new StreamReader(bs, detectEncodingFromByteOrderMarks: true))
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
