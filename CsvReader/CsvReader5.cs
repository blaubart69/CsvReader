using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Spi
{
    public class CsvReader5
    {
        struct Field
        {
            public int startIdx;
            public int len;
            public int QuoteCount;
        }

        TextReaderBufferEspecial _reader;
        readonly Func<TextReaderBufferEspecial> readerFactory;

        readonly char FieldDelimiter;

        const int INITIAL_FIELDS_SIZE = 8;
        const char DOUBLE_QUOTE = '\"';

        Field[] _fields;

        int _fieldIdx_toWrite;

        readonly StringBuilder dbg = new StringBuilder();

        public CsvReader5(TextReader reader, char fieldDelimiter = ',', int buffersize = 4096)
        {
            readerFactory = () => new TextReaderBufferEspecial(reader, buffersize);
            FieldDelimiter = fieldDelimiter;
            _fields = new Field[INITIAL_FIELDS_SIZE];
        }
        public bool Read()
        {
            if ( _reader == null )
            {
                _reader = readerFactory();
            }

            if ( _reader.EOF() )
            {
                return false;
            }

            _fieldIdx_toWrite = 0;

            return ReadOneRecord();
        }
        private bool ReadOneRecord()
        {
            char c = '\0';
            char lastChar = FieldDelimiter;

            while (true)
            {
                if (_reader.Read(ref c))
                {

                }
                else // EOF
                {

                }
            }
        }
        /*
        private bool ReadOneRecord()
        {
            int readIdx = 0;
            int fieldIdxStart = 0;
            char? lastChar = FieldDelimiter;
            int quoteCount = 0;
            bool recordFinished = false;
            bool inQuotedField = false;

            dbg.Length = 0;

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
                    if (lastChar == null && c == DOUBLE_QUOTE)
                    {
                        inQuotedField = false;
                    }
                    else
                    {
                        for (;;)
                        {
                            if (lastChar == DOUBLE_QUOTE)
                            {
                                if (c == DOUBLE_QUOTE)
                                {
                                    ++quoteCount;
                                }
                                else if (c == FieldDelimiter || c == '\n' || c == '\r')
                                {
                                    inQuotedField = false;
                                    AddField(startIdx: fieldIdxStart,
                                        len: (readIdx - fieldIdxStart) - 1,
                                        quoteCount);
                                    fieldIdxStart = readIdx + 1;
                                    break;
                                }
                            }
                            ++readIdx;
                            readIdxAbsolut = _recordStartIdx_Read + readIdx;
                            if (readIdxAbsolut >= _bufLen)
                            {
                                if ( _bufLen < BUFSIZE )
                                {
                                    AddField(startIdx: fieldIdxStart,
                                       len: (readIdx - fieldIdxStart) - 1,
                                       quoteCount);
                                    inQuotedField = false;
                                }
                                break;
                            }

                            lastChar = c;
                            c = _buf[readIdxAbsolut];
                        }
                    }
                }
                else
                {
                    if (c == '\n' || c == '\r')
                    {
                        if (lastChar != '\n' && lastChar != '\r')
                        {
                            int truncate = lastChar == DOUBLE_QUOTE ? 1 : 0;
                            AddField(startIdx:      fieldIdxStart,
                                     len:           (readIdx - fieldIdxStart) - truncate,
                                     quoteCount);
                            quoteCount = 0;
                        }
                        if (c == '\n')
                        {
                            recordFinished = true;
                        }
                    }
                    else if (c == FieldDelimiter)
                    {
                        AddField(   startIdx:   fieldIdxStart,
                                    len:        (readIdx - fieldIdxStart),
                                    quoteCount);
                        quoteCount = 0;

                        fieldIdxStart = readIdx + 1;
                    }
                    else if (c == DOUBLE_QUOTE)
                    {
                        if (lastChar == FieldDelimiter)
                        {
                            ++fieldIdxStart;
                            inQuotedField = true;
                            lastChar = null;
                            goto dirty;
                        }
                        else
                        {
                            Console.WriteLine($"c\t[{c}]\nlastChar\t[{lastChar}]\ninQuotedField\t{inQuotedField}\nbuflen\t{_bufLen}\n_recordStartIdx_Read\t{_recordStartIdx_Read}\nreadIdx\t{readIdx}\nreadIdxAbsolut\t{readIdxAbsolut}\n_fieldIdx_toWrite\t{_fieldIdx_toWrite}\nfieldIdxStart\t{fieldIdxStart}");
                            for (int i=0; i <_fieldIdx_toWrite; ++i)
                            {
                                Console.WriteLine($"fieldIdx({i})\t[[[{this[i].ToString()}]]]");
                            }
                            string errRecord = new string(_buf, _recordStartIdx_Read, readIdx+1).Replace("\r\n","__");
                            Console.WriteLine(dbg.ToString());
                            throw new Exception($"quotes are not allowed within unquoted fields. [{errRecord}]");
                        }
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
                if (inQuotedField)
                {
                    throw new Exception("missing end quote");
                }
                AddField(fieldIdxStart, (readIdx - fieldIdxStart), quoteCount);

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
                    dbg.AppendLine("HandleReadIdxBehindBuffer - numCharsRead == 0");
                    recordFinished = true;
                }

                _recordStartIdx_Read = 0;
                _recordStartIdx_Get = 0;
            }

            dbg.AppendLine($"HandleReadIdxBehindBuffer {recordFinished}");

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
        */
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
