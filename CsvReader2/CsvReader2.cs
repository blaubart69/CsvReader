using System;
using System.Collections.Generic;
using System.IO;

namespace Spi
{
    struct Field
    {
        public int startIdx;
        public int endIdx;
        public int QuoteCount;
    }
    public class CsvReader2
    {
        readonly TextReader _reader;
        readonly char       _fieldDelimiter;

        readonly int BUFSIZE;
        const int INITIAL_FIELDS_SIZE = 8;
        const char DOUBLE_QUOTE = '\"';
        
        Field[] _fields;

        char[]  _buf;
        int     _bufLen;
        int     _recordStartIdx;
        bool    _eof;
        int     _fieldCount;

        public CsvReader2(TextReader reader, char fieldDelimiter, int buffersize = 4096)
        {
            _reader = reader;
            _fieldDelimiter = fieldDelimiter;
            BUFSIZE = buffersize;
        }
        public bool Read()
        {
            if (_buf == null)
            {
                Init();
            }

            int readIdxRelative         = 0;
            int fieldIdxStartRelative   = 0;
               _fieldCount              = 0;

            char? lastChar = _fieldDelimiter;
            bool setLastCharToNull;

            bool inQuotes = false;
            int quoteCount = 0;

            for (;;)
            {
                setLastCharToNull = false;
                int readIdxAbsolut = _recordStartIdx + readIdxRelative;
                if (readIdxAbsolut >= _bufLen)
                {
                    if (_eof || ShiftLeftAndFillBuffer() == 0)
                    {
                        // gaunz aus
                        AddField(fieldIdxStartRelative, readIdxRelative, QuoteCount: 0);
                        break;
                    }
                }

                char c = _buf[readIdxAbsolut];

                if      ( c == _fieldDelimiter )
                {
                    HandleFieldDelimiter(readIdxRelative, ref fieldIdxStartRelative, lastChar, ref inQuotes);
                }
                else if ( c == DOUBLE_QUOTE )
                {
                    HandleDoubleQuote(lastChar, ref setLastCharToNull, ref inQuotes, ref quoteCount, ref fieldIdxStartRelative);
                }
                else if ( c == '\n' || c == '\r')
                {
                    bool EndOfRecord = HandleEndOfRecord(ref inQuotes, c, lastChar, quoteCount, fieldIdxStartRelative, readIdxRelative);
                    if ( EndOfRecord )
                    {
                        break;
                    }
                }

                lastChar = setLastCharToNull ? (char?)null : c;
                ++readIdxRelative;
            }
        }

        private bool HandleEndOfRecord(ref bool inQuotes, char currChar, char? lastChar, int quoteCount, int fieldIdxStartRelative, int readIdxRelative)
        {
            if (inQuotes)
            {
                if (lastChar == DOUBLE_QUOTE)
                {
                    inQuotes = false;
                    AddField(fieldIdxStartRelative, readIdxRelative - 2, quoteCount);
                }
            }
            else
            {
                if (lastChar != '\n' && lastChar != '\r')
                {
                    AddField(fieldIdxStartRelative, readIdxRelative - 1, quoteCount);
                }
                if (currChar == '\n')
                {
                    return true;
                }
            }

            return false;
        }

        private void HandleDoubleQuote(char? lastChar, ref bool setLastCharToNull, ref bool inQuotes, ref int quoteCount, ref int fieldIdxStartRelative)
        {
            if (lastChar == _fieldDelimiter)
            {
                inQuotes = true;
                setLastCharToNull = true;
                ++fieldIdxStartRelative;
            }
            else if (lastChar == DOUBLE_QUOTE)
            {
                ++quoteCount;
            }
        }

        private void HandleFieldDelimiter(int readIdxRelative, ref int fieldIdxStartRelative, char? lastChar, ref bool inQuotes)
        {
            if (inQuotes)
            {
                if (lastChar == DOUBLE_QUOTE)
                {
                    inQuotes = false;
                    AddField(fieldIdxStartRelative, readIdxRelative - 2, 0);
                    fieldIdxStartRelative = readIdxRelative + 1;
                }
            }
            else
            {
                if (lastChar == DOUBLE_QUOTE)
                {
                    // field ended with quote but not started with qoute ==> error
                }
                else
                {
                    AddField(fieldIdxStartRelative, readIdxRelative - 1, 0);
                    fieldIdxStartRelative = readIdxRelative + 1;
                }
            }
        }

        private int ShiftLeftAndFillBuffer()
        {
            if (_recordStartIdx == 0)
            {
                throw new OutOfMemoryException("buffer exhausted to handle this CSV record");
            }
            //
            // move the actual record down the array
            //
            int charsToMoveDown = _bufLen - _recordStartIdx;
            Array.Copy(
                sourceArray: _buf,
                sourceIndex: _recordStartIdx,
                destinationArray: _buf,
                destinationIndex: 0,
                length: charsToMoveDown);
            //
            // fill the rest of the array
            //
            int charsToReadFromReader = BUFSIZE - charsToMoveDown;
            int numberCharRead = _reader.ReadBlock(_buf, charsToMoveDown, charsToReadFromReader);

            if (numberCharRead < charsToReadFromReader)
            {
                _eof = true;
            }

            _recordStartIdx = 0;
            _bufLen = charsToMoveDown + numberCharRead;

            return numberCharRead;
        }
        private void Init()
        {
            _buf = new char[BUFSIZE];
            _bufLen = _reader.ReadBlock(_buf);

            if (_bufLen < BUFSIZE)
            {
                _eof = true;
            }

            _recordStartIdx = 0;
            _fields = new Field[INITIAL_FIELDS_SIZE];
        }

        private void AddField(int fieldStartIdx, int fieldEndIdx, int QuoteCount)
        {
            ++_fieldCount;
            if ( _fieldCount > _fields.Length)
            {
                Array.Resize(ref _fields, _fields.Length * 4);
            }

            int idx = _fieldCount - 1;
            _fields[idx].startIdx   = fieldStartIdx;
            _fields[idx].endIdx     = fieldEndIdx;
            _fields[idx].QuoteCount = QuoteCount;
        }
    }
}
