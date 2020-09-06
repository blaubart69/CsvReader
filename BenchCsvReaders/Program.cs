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
        //[Benchmark]
        public void Spi_Reader_V4_dir__4k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\allc.tsv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new Spi.CsvReader5(rdr, '\t');
                while (csvrdr.Read())
                {
                }
            }
        }
        [Benchmark]
        public void Spi_Reader_V4_dir_32k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\allc.tsv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new Spi.CsvReader5(rdr, '\t', buffersize: 32*1024);
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
        public void Spi_Reader_V4_5MB_quoted_64k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\2.csv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new Spi.CsvReader5(rdr, ';', buffersize: 64*1024);
                while (csvrdr.Read())
                {
                }
            }
        }

        //[Benchmark]
        public void NReco_5MB_quoted_64k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\2.csv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new NReco.Csv.CsvReader(rdr, ";") { TrimFields=false, BufferSize=64*1024 };
                while (csvrdr.Read())
                {
                }
            }
        }
        //[Benchmark]
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
        //[Benchmark]
        public void Spi_Reader_V4_dir_quoted_4k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\allc_quoted.csv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new Spi.CsvReader5(rdr, ',');
                while (csvrdr.Read())
                {
                }
            }
        }
        [Benchmark]
        public void Spi_Reader_V4_dir_quoted_32k()
        {
            using (TextReader rdr = new StreamReader(@"c:\temp\allc_quoted.csv", detectEncodingFromByteOrderMarks: true))
            {
                var csvrdr = new Spi.CsvReader5(rdr, ',', buffersize: 32*1024);
                while (csvrdr.Read())
                {
                }
            }
        }
        [Benchmark]
        public void NReco_dir_quoted_32k()
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
