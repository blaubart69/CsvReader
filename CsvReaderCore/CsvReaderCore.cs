using System;
using System.Collections.Generic;
using System.IO;

namespace Spi
{
    public class CsvReaderCore
    {
        readonly TextReader _reader;
        readonly char _fieldDelimiter;
        const int BUFSIZE = 32 * 1024;

        char[] _buf                 = new char[BUFSIZE];
        List<Memory<char>> field    = new List<Memory<char>>();

        int fieldcount;
        int readpos;
        int buf_start_idx;
        int buf_len;

        public CsvReaderCore(TextReader reader, char fieldDelimiter)
        {
            _reader = reader;
            _fieldDelimiter = fieldDelimiter;
        }
        public bool Read()
        {
            if ( buf_len == 0 )
            {
                _reader.ReadBlock()
            }
            
        }
    }
}
