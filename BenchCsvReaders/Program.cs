using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using System;
using System.IO;

namespace BenchCsvReaders
{
    [MemoryDiagnoser]
    //[InliningDiagnoser(logFailuresOnly: false, filterByNamespace: false)]
    //[DisassemblyDiagnoser(maxDepth:3,printSource:true)]
    //[HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses, HardwareCounter.BranchInstructions)]
    public class Benches
    {
        //[Benchmark]
        public void Spi_Reader_V1_dir()
        {
            using (var bs = new BufferedStream(new FileStream(@"c:\temp\allc.tsv", FileMode.Open, FileAccess.Read)))
            using (TextReader rdr = new StreamReader(bs, detectEncodingFromByteOrderMarks: true))
            {
                Spi.CsvReader.Run(rdr, '\t',
                    OnRow: (string[] fields) =>
                    {
                    });
            }
        }
        [Benchmark]
        public void Spi_Reader_V3_dir__4k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\allc.tsv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new Spi.CsvReader3(rdr, '\t');
                while (csvrdr.Read())
                {
                }
            }
        }
        [Benchmark]
        public void Spi_Reader_V3_dir_32k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\allc.tsv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new Spi.CsvReader3(rdr, '\t', buffersize: 32*1024);
                while (csvrdr.Read())
                {
                }
            }
        }
        //[Benchmark]
        public void Spi_Reader_V3_dir_64k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\allc.tsv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new Spi.CsvReader3(rdr, '\t', buffersize: 64 * 1024);
                while (csvrdr.Read())
                {
                }
            }
        }

        [Benchmark]
        public void NReco_dir_32k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\allc.tsv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new NReco.Csv.CsvReader(rdr, "\t") { TrimFields = false };
                while (csvrdr.Read())
                {
                }
            }
        }
        //[Benchmark]
        public void NReco_dir_64k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\allc.tsv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new NReco.Csv.CsvReader(rdr, "\t") { TrimFields = false, BufferSize=64*1024 };
                while (csvrdr.Read())
                {
                }
            }
        }
        //[Benchmark]
        public void Spi_Reader_V3_5MB_quoted_64k()
        {
            using (TextReader rdr = new StreamReader(@"c:\Users\bee\Downloads\2.csv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new Spi.CsvReader3(rdr, ';', buffersize: 64*1024);
                while (csvrdr.Read())
                {
                }
            }
        }

        //[Benchmark]
        public void NReco_5MB_quoted_64k()
        {
            using (TextReader rdr = new StreamReader(@"c:\Users\bee\Downloads\2.csv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new NReco.Csv.CsvReader(rdr, ";") { BufferSize = 64 * 1024, TrimFields=false };
                while (csvrdr.Read())
                {
                }
            }
        }
        [Benchmark]
        public void Spi_Reader_V3_dir_quoted_32k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\allc_quoted.csv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new Spi.CsvReader3(rdr, ',', buffersize: 32 * 1024);
                while (csvrdr.Read())
                {
                }
            }
        }

        [Benchmark]
        public void NReco_5MB_dir_quoted_32k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\allc_quoted.csv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new NReco.Csv.CsvReader(rdr, ",") { TrimFields = false };
                while (csvrdr.Read())
                {
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
