using System;
using System.IO;

namespace Spi
{
    public class CsvReader3
    {
        struct Field
        {
            public int startIdx;
            public int len;
            public int QuoteCount;
        }

        readonly TextReader _reader;
        readonly char FieldDelimiter;

        readonly int BUFSIZE;
        const int INITIAL_FIELDS_SIZE = 8;
        const char DOUBLE_QUOTE = '\"';

        Field[] _fields;

        char[] _buf;
        int _bufLen;
        int _recordStartIdx;
        bool _eof;
        int _fieldIdx_lastElement;

        public CsvReader3(TextReader reader, char fieldDelimiter, int buffersize = 4096)
        {
            _reader = reader;
            FieldDelimiter = fieldDelimiter;
            BUFSIZE = buffersize;
        }
        public bool Read()
        {
            if (_buf == null)
            {
                Init();
            }

            int readIdx = 0;
            int fieldIdxStart = 0;
            char? lastChar = FieldDelimiter;
            bool setLastChar;
            bool inQuotes = false;
            int quoteCount = 0;

            _fieldIdx_lastElement = 0;

            for (;;)
            {
                setLastChar = true;
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

                if (inQuotes)
                {
                    if (lastChar == DOUBLE_QUOTE)
                    {
                        if (c == DOUBLE_QUOTE)
                        {
                            ++quoteCount;
                            setLastChar = false;
                        }
                        else if (c == FieldDelimiter)
                        {
                            AddField(startIdx: fieldIdxStart, len: (readIdx - fieldIdxStart) - 2, quoteCount);
                            quoteCount = 0;
                            fieldIdxStart = readIdx + 1;
                            inQuotes = false;
                        }
                    }
                }
                else
                {
                    if (lastChar == FieldDelimiter && c == DOUBLE_QUOTE)    //  begin of quoted field
                    {
                        inQuotes = true;
                        setLastChar = false;
                        ++fieldIdxStart;
                    }
                    else if (c == FieldDelimiter)
                    {
                        if (lastChar == DOUBLE_QUOTE)
                        {
                            // field ended with quote but not started with quote ==> error
                        }
                        else
                        {
                            // unquoted field ended
                            AddField(
                                startIdx:   fieldIdxStart,
                                len:        (readIdx - fieldIdxStart) - 1, // -1 ... cut off the field delimiter
                                QuoteCount: 0);
                            fieldIdxStart = readIdx + 1;
                        }
                    }
                    else if (c == '\n' || c == '\r')
                    {
                        if (lastChar != '\n' && lastChar != '\r')
                        {
                            AddField(startIdx: fieldIdxStart, len: readIdx - 1, quoteCount);
                        }
                        if (c == '\n')
                        {
                            break;
                        }
                    }
                }

                lastChar = setLastChar ? c : (char?)null;
                ++readIdx;
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

        private void AddField(int startIdx, int len, int QuoteCount)
        {
            if (_fieldIdx_lastElement == _fields.Length)
            {
                Array.Resize(ref _fields, _fields.Length * 4);
            }

            _fields[_fieldIdx_lastElement].startIdx = startIdx;
            _fields[_fieldIdx_lastElement].len = len;
            _fields[_fieldIdx_lastElement].QuoteCount = QuoteCount;

            ++_fieldIdx_lastElement;
        }
    }
}
