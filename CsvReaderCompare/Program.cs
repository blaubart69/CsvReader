﻿using System;
using System.IO;

namespace CsvReaderCompare
{
    class Program
    {
        static void Main(string[] args)
        {
            string delim = args[0];
            string filename = args[1];

            using (TextReader r1 = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read), detectEncodingFromByteOrderMarks: true))
            using (TextReader r2 = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read), detectEncodingFromByteOrderMarks: true))
            {
                var spi    = new Spi.CsvReader5(r1, delim[0], buffersize: 64*1024);
                var gegner = new NReco.Csv.NRecoCsvReader(r2, delim) { TrimFields = false, BufferSize=64*1024 };
                int record = 0;

                try
                {
                    for (; ; )
                    {

                        if (!spi.Read() | !gegner.Read())
                        {
                            break;
                        }

                        ++record;

                        if (spi.FieldCount != gegner.FieldsCount)
                        {
                            Console.WriteLine($"diff fieldcount - record: {record}\tfields: spi {spi.FieldCount}/{gegner.FieldsCount}");
                        }

                        for (int i = 0; i < spi.FieldCount; ++i)
                        {
                            string s = spi[i].ToString();
                            string g = gegner[i];
                            if (String.CompareOrdinal(s, g) != 0)
                            {
                                Console.WriteLine($"--- {record} ---\ndiff s: [{s}]\ndiff g: [{g}]\n--- {record} ---");
                            }
                        }

                        //spi.PrintDebugInfo();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"records parsed: {record}\nException: {ex.Message}");
                    spi.PrintDebugInfo();
                }
                Console.WriteLine($"compared {record} records");
            }
        }
    }
}
