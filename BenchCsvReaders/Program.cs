using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using System;
using System.IO;

namespace BenchCsvReaders
{
    [MemoryDiagnoser]
    public class Benches
    {
        [Benchmark]
        public void Spindi()
        {
            Consumer Konsum = new Consumer();
            using (var bs = new BufferedStream(new FileStream(@"c:\temp\allc.tsv", FileMode.Open, FileAccess.Read)))
            using (TextReader rdr = new StreamReader(bs, detectEncodingFromByteOrderMarks: true))
            {
                Spi.CsvReader.Run(rdr, '\t',
                    OnRow: (string[] fields) =>
                    {
                        for (int i = 0; i < fields.Length; ++i)
                        {
                            Konsum.Consume(fields[i]);
                        }
                    });
            }
        }
        [Benchmark]
        public void CircularCsv()
        {
            Consumer Konsum = new Consumer();
            using (TextReader rdr = new StreamReader(@"c:\temp\allc.tsv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new NReco.Csv.CsvReader(rdr, "\t");
                while (csvrdr.Read())
                {
                    for (int i = 0; i < csvrdr.FieldsCount; ++i)
                    {
                        Konsum.Consume(csvrdr[i]);
                    }
                }
            }
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run(typeof(Benches).Assembly);
        }
    }
}
