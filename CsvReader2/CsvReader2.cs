using System;
using System.Collections.Generic;
using System.IO;

namespace Spi
{
    struct Field
    {
        public int startIdx;
        public int len;
        public int QuoteCount;
    }
    public class CsvReader2
    {
        readonly TextReader _reader;
        readonly char       _fieldDelimiter;

        readonly int  BUFSIZE;
        const    int  INITIAL_FIELDS_SIZE = 8;
        const    char DOUBLE_QUOTE = '\"';
        
        Field[] _fields;

        char[]  _buf;
        int     _bufLen;
        int     _recordStartIdx;
        bool    _eof;
        int     _fieldIdx_lastElement;

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

            int   readIdx         = 0;
            int   fieldIdxStart   = 0;
            char? lastChar = _fieldDelimiter;
            bool  setLastCharToNull;
            bool  inQuotes = false;
            int   quoteCount = 0;

            _fieldIdx_lastElement = 0;

            for (;;)
            {
                setLastCharToNull = false;
                int readIdxAbsolut = _recordStartIdx + readIdx;
                if (readIdxAbsolut >= _bufLen)
                {
                    if (_eof || ShiftLeftAndFillBuffer() == 0)
                    {
                        // gaunz aus
                        AddField(fieldIdxStart, readIdx, QuoteCount: 0);
                        break;
                    }
                }

                char c = _buf[readIdxAbsolut];

                if      ( c == _fieldDelimiter )
                {
                    HandleFieldDelimiter(readIdx, lastChar, ref fieldIdxStart, ref inQuotes);
                }
                else if ( c == DOUBLE_QUOTE )
                {
                    HandleDoubleQuote(lastChar, ref setLastCharToNull, ref inQuotes, ref quoteCount, ref fieldIdxStart);
                }
                else if ( c == '\n' || c == '\r')
                {
                    bool EndOfRecord = HandleEndOfRecord(c, lastChar, quoteCount, fieldIdxStart, readIdx, ref inQuotes);
                    if ( EndOfRecord )
                    {
                        break;
                    }
                }

                lastChar = setLastCharToNull ? (char?)null : c;
                ++readIdx;
            }
        }

        private bool HandleEndOfRecord(char currChar, char? lastChar, int quoteCount, int fieldIdxStart, int readIdx, ref bool inQuotes)
        {
            if (inQuotes)
            {
                if (lastChar == DOUBLE_QUOTE)
                {
                    inQuotes = false;
                    AddField(startIdx: fieldIdxStart, len: readIdx - 2, quoteCount);
                }
            }
            else
            {
                if (lastChar != '\n' && lastChar != '\r')
                {
                    AddField(startIdx: fieldIdxStart, len: readIdx - 1, quoteCount);
                }
                if (currChar == '\n')
                {
                    return true;
                }
            }

            return false;
        }

        private void HandleDoubleQuote(char? lastChar, ref bool setLastCharToNull, ref bool inQuotes, ref int quoteCount, ref int fieldIdxStart)
        {
            if (lastChar == _fieldDelimiter)
            {
                inQuotes = true;
                setLastCharToNull = true;
                ++fieldIdxStart;
            }
            else if (lastChar == DOUBLE_QUOTE)
            {
                ++quoteCount;
            }
        }

        private void HandleFieldDelimiter(int readIdx, char? lastChar, ref int fieldIdxStart, ref bool inQuotes)
        {
            if (inQuotes)
            {
                if (lastChar == DOUBLE_QUOTE)
                {
                    inQuotes = false;
                    AddField(fieldIdxStart, readIdx - 2, 0);
                    fieldIdxStart = readIdx + 1;
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
                    AddField( 
                        startIdx:   fieldIdxStart, 
                        len:        (readIdx - fieldIdxStart) - 1, // -1 ... cut of the field delimiter
                        QuoteCount: 0);
                    fieldIdxStart = readIdx + 1;
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
                sourceArray:        _buf,
                sourceIndex:        _recordStartIdx,
                destinationArray:   _buf,
                destinationIndex:   0,
                length:             charsToMoveDown);
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

        private void AddField(int startIdx, int len, int QuoteCount)
        {
            if ( _fieldIdx_lastElement == _fields.Length )
            {
                Array.Resize(ref _fields, _fields.Length * 4);
            }

            _fields[_fieldIdx_lastElement].startIdx   = startIdx;
            _fields[_fieldIdx_lastElement].len        = len;
            _fields[_fieldIdx_lastElement].QuoteCount = QuoteCount;

            ++_fieldIdx_lastElement;
        }
    }
}
