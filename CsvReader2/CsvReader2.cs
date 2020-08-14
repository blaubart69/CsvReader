using System;
using System.Collections.Generic;
using System.IO;

namespace Spi
{
    struct Field
    {
        public int startIdx;
        public int endIdx;
        public bool isQuoted;
        public int QuoteCount;
    }
    public class CsvReader2
    {
        readonly TextReader _reader;
        readonly char       _fieldDelimiter;

        const int BUFSIZE = 32 * 1024;
        const int INITIAL_FIELDS_SIZE = 8;
        const char DOUBLE_QUOTE = '\"';
        
        Field[] _fields;

        char[]  _buf;
        int     _bufLen;
        int     _recordStartIdx;
        bool    _eof;

        public CsvReader2(TextReader reader, char fieldDelimiter)
        {
            _reader = reader;
            _fieldDelimiter = fieldDelimiter;
        }

        public bool Read()
        {
            // ----------------------------------------------------------------
            if (_buf == null)
            // ----------------------------------------------------------------
            {
                _buf = new char[BUFSIZE];
                _bufLen = _reader.ReadBlock(_buf);
                
                if (_bufLen < BUFSIZE )
                {
                    _eof = true;
                }

                _recordStartIdx = 0;
                _fields = new Field[INITIAL_FIELDS_SIZE];
            }

            int fieldCount      = 0;
            int readIdxRelative = 0;
            int fieldStartIdx   = 0;
            int fieldEndIdx     = -1;

            char lastChar = _fieldDelimiter;

            for (;;)
            {
                int readIdxAbsolut = _recordStartIdx + readIdxRelative;
                if (readIdxAbsolut >= _bufLen)
                {
                    if (_eof)
                    {
                        // gaunz aus
                        AddField(fieldStartIdx, fieldEndIdx, isQuoted: false, QuoteCount: 0, ref fieldCount);
                        break;
                    }
                    else
                    {
                        if ( _recordStartIdx == 0 )
                        {
                            throw new OutOfMemoryException("buffer exhausted to handle this CSV record");
                        }
                        //
                        // move the actual record down the array
                        //
                        int charsToMoveDown       = _bufLen - _recordStartIdx;
                        Array.Copy(
                            sourceArray:        _buf,
                            sourceIndex:        _recordStartIdx,
                            destinationArray:   _buf,
                            destinationIndex:   0,
                            length:             charsToMoveDown);
                        //
                        // fill up the rest of the array
                        //
                        int charsToReadFromReader = BUFSIZE - charsToMoveDown;
                        int numberCharRead = _reader.ReadBlock(_buf, charsToMoveDown, charsToReadFromReader);

                        if (numberCharRead < charsToReadFromReader)
                        {
                            _eof = true;
                        }

                        _recordStartIdx = 0;
                        _bufLen = charsToMoveDown + numberCharRead;
                    }
                }

                char c = _buf[readIdxAbsolut];     // memory access

                if ( c == DOUBLE_QUOTE )
                {

                }

                ++readIdxRelative;
            }

        }

        private void AddField(int fieldStartIdx, int fieldEndIdx, bool isQuoted, int QuoteCount, ref int fieldCount)
        {
            ++fieldCount;
            if ( fieldCount > _fields.Length)
            {
                Array.Resize(ref _fields, _fields.Length * 4);
            }

            int idx = fieldCount - 1;
            _fields[idx].startIdx   = fieldStartIdx;
            _fields[idx].endIdx     = fieldEndIdx;
            _fields[idx].isQuoted   = isQuoted;
            _fields[idx].QuoteCount = QuoteCount;
        }
    }
}
