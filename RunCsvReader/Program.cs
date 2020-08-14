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
                StringBuilder sb = new StringBuilder();
                Spi.CsvReader.Run(rdr, '\t',
                    OnRow: (string[] fields) =>
                    {
                        for (int i = 0; i < fields.Length; ++i)
                        {
                            sb.Append(fields[i]);
                        }
                        sb.Length = 0;
                    });
            }
        }
    }
}
