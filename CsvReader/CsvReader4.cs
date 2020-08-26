using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Spi
{
    public class CsvReader4
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
        int _recordStartIdx_Read;
        int _recordStartIdx_Get;
        int _fieldIdx_toWrite;

        public CsvReader4(TextReader reader, char fieldDelimiter = ',', int buffersize = 4096)
        {
            _reader = reader;
            FieldDelimiter = fieldDelimiter;
            BUFSIZE = buffersize;
        }
        public bool Read()
        {
            if (_buf == null)
            {
                _buf = new char[BUFSIZE];
                _bufLen = _reader.ReadBlock(_buf);
                _recordStartIdx_Read = 0;
                _fields = new Field[INITIAL_FIELDS_SIZE];
            }

            _fieldIdx_toWrite = 0;

            if (_recordStartIdx_Read == BUFSIZE)
            {
            }
            else if (_recordStartIdx_Read >= _bufLen)
            {
                return false;
            }

            _recordStartIdx_Get = _recordStartIdx_Read;

            int recordLength = ReadOneRecord();
            _recordStartIdx_Read += recordLength;

            return true;
        }
        private int ReadOneRecord()
        {
            int readIdx = 0;
            int fieldIdxStart = 0;
            char? lastChar = FieldDelimiter;
            int quoteCount = 0;
            bool recordFinished = false;
            bool inQuotedField = false;

            while (!recordFinished)
            {
                int readIdxAbsolut = _recordStartIdx_Read + readIdx;
                if (readIdxAbsolut >= _bufLen)
                {
                    recordFinished = HandleReadIdxBehindBuffer(readIdx, fieldIdxStart, quoteCount, recordFinished, lastChar, inQuotedField);
                    if (recordFinished)
                    {
                        goto dirty; // skip reading character
                    }
                    if (_recordStartIdx_Read == 0)
                    {
                        readIdxAbsolut = 0 + readIdx;
                    }
                }

                char c = _buf[readIdxAbsolut];

                if (inQuotedField)
                {
                    for (;;)
                    {
                        if (lastChar == DOUBLE_QUOTE)
                        {
                            if (c == DOUBLE_QUOTE)
                            {
                                ++quoteCount;
                                lastChar = null;
                                goto donotsetlastchar;
                            }
                            else if (c == FieldDelimiter || c == '\n' || c == '\r')
                            {
                                AddField(startIdx: fieldIdxStart,
                                            len: (readIdx - fieldIdxStart) - 1,
                                            quoteCount);

                                inQuotedField = false;
                                quoteCount = 0;
                                fieldIdxStart = readIdx + 1;

                                if (c == '\n')
                                {
                                    recordFinished = true;
                                }

                                break;
                            }
                        }

                        lastChar = c;

                    donotsetlastchar:

                        ++readIdx;
                        readIdxAbsolut = _recordStartIdx_Read + readIdx;
                        if (readIdxAbsolut >= _bufLen)
                        {
                            --readIdx;
                            break;
                        }
                        c = _buf[readIdxAbsolut];
                    }
                }
                else
                {
                    if (c == '\n' || c == '\r')
                    {
                        if (lastChar != '\n' && lastChar != '\r')
                        {
                            AddField(startIdx:      fieldIdxStart,
                                     len:           readIdx - fieldIdxStart,
                                     quoteCount:    0);
                        }
                        if (c == '\n')
                        {
                            recordFinished = true;
                        }
                    }
                    else if (c == FieldDelimiter)
                    {
                        AddField(   startIdx:   fieldIdxStart,
                                    len:        readIdx - fieldIdxStart,
                                    quoteCount: 0);

                        fieldIdxStart = readIdx + 1;
                    }
                    else if (lastChar == FieldDelimiter && c == DOUBLE_QUOTE)
                    {
                        ++fieldIdxStart;
                        inQuotedField = true;
                        lastChar = null;
                        goto dirty;
                        // loop until end of quoted field


                    }
                    else if (c == DOUBLE_QUOTE)
                    {
                        throw new Exception("quotes are not allowed within unquoted fields");
                    }
                }

                lastChar = c;

            dirty:
                ++readIdx;
            }

            return readIdx;
        }
        private bool HandleReadIdxBehindBuffer(int readIdx, int fieldIdxStart, int quoteCount, bool recordFinished, char? lastChar, bool inQuotedField)
        {
            if (_bufLen < BUFSIZE)
            {
                // EOF reached since the buffer is not full
                // last record
                if (inQuotedField)
                {
                    if (lastChar == DOUBLE_QUOTE)
                    {
                        AddField(fieldIdxStart, (readIdx - fieldIdxStart) - 1, quoteCount);
                    }
                    else
                    {
                        throw new Exception("missing end quote");
                    }
                }
                else
                {
                    AddField(fieldIdxStart, readIdx - fieldIdxStart, quoteCount);
                }

                recordFinished = true;
            }
            else
            {
                if (_recordStartIdx_Read == 0)
                {
                    throw new Exception($"buffer exhausted to handle this CSV record. buffer [{new string(_buf)}]");
                }

                int numberCharsRead = ShiftBufferAndRefill();
                if (numberCharsRead == 0)
                {
                    AddField(fieldIdxStart, readIdx, quoteCount);
                    recordFinished = true;
                }

                _recordStartIdx_Read = 0;
                _recordStartIdx_Get = 0;
            }

            return recordFinished;
        }
        private int ShiftBufferAndRefill()
        {
            //
            // move the actual record down the array
            //
            int charsToMoveDown = _bufLen - _recordStartIdx_Read;
            if (charsToMoveDown > 0)
            {
                Array.Copy(
                    sourceArray: _buf,
                    sourceIndex: _recordStartIdx_Read,
                    destinationArray: _buf,
                    destinationIndex: 0,
                    length: charsToMoveDown);
            }
            //
            // fill the rest of the array
            //
            int charsToRead = BUFSIZE - charsToMoveDown;
            int numberCharsRead = _reader.ReadBlock(_buf, charsToMoveDown, charsToRead);

            _bufLen = charsToMoveDown + numberCharsRead;

            return numberCharsRead;
        }
        private void AddField(int startIdx, int len, int quoteCount)
        {
            if (_fieldIdx_toWrite == _fields.Length)
            {
                Array.Resize(ref _fields, _fields.Length * 4);
            }

            _fields[_fieldIdx_toWrite].startIdx = startIdx;
            _fields[_fieldIdx_toWrite].len = len;
            _fields[_fieldIdx_toWrite].QuoteCount = quoteCount;

            ++_fieldIdx_toWrite;
        }
        #region FIELD_ACCESS
        public ReadOnlySpan<char> this[int idx]
        {
            get
            {
                if (idx <= _fieldIdx_toWrite)
                {
                    Field f = _fields[idx];

                    var fieldValue =
                        _buf.AsSpan(start: _recordStartIdx_Get + f.startIdx,
                                    length: f.len);

                    if (f.QuoteCount == 0)
                    {
                        return fieldValue;
                    }
                    else
                    {
                        return fieldValue.ToString().Replace("\"\"", "\"").AsSpan();
                    }
                }

                return null;
            }
        }
        public ReadOnlySpan<char> GetRawValue(int idx)
        {
            if (idx <= _fieldIdx_toWrite)
            {
                Field f = _fields[idx];

                return
                    _buf.AsSpan(
                        start: _recordStartIdx_Get + f.startIdx,
                        length: f.len);
            }

            return null;
        }
        public int FieldCount
        {
            get
            {
                return _fieldIdx_toWrite;
            }
        }
        #endregion

    }
}
