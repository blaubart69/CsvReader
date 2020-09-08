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

        BufferEspecial _buffer;
        readonly Func<BufferEspecial> readerFactory;

        readonly char FieldDelimiter;

        const int INITIAL_FIELDS_SIZE = 8;
        const char DOUBLE_QUOTE = '\"';

        Field[] _fields;

        int _fieldIdx_toWrite;

        readonly StringBuilder dbg = new StringBuilder();

        public CsvReader5(TextReader reader, char fieldDelimiter = ',', int buffersize = 4096)
        {
            readerFactory = () => new BufferEspecial(reader, buffersize);
            FieldDelimiter = fieldDelimiter;
            _fields = new Field[INITIAL_FIELDS_SIZE];
        }
        public bool Read()
        {
            if ( _buffer == null )
            {
                _buffer = readerFactory();
            }

            if ( _buffer.EOF() )
            {
                return false;
            }

            _fieldIdx_toWrite = 0;
            _buffer.SetStartIdx();

            ReadOneRecord();

            return true;
        }
        private void ReadOneRecord()
        {
            char c = '\0';
            char lastChar = FieldDelimiter;
            int fieldIdxStart = 0;
            int quoteCount = 0;

            while (_buffer.Read(ref c))
            {
                if (c == '\n' || c == '\r')
                {
                    if (lastChar != '\n' && lastChar != '\r')
                    {
                        AddField(fieldIdxStart, _buffer.LastReadIdx(), lastChar == DOUBLE_QUOTE, quoteCount);
                    }
                    if (c == '\n')
                    {
                        return;
                    }
                }
                else if (c == FieldDelimiter)
                {
                    AddField(fieldIdxStart, _buffer.LastReadIdx(), lastChar == DOUBLE_QUOTE, quoteCount);
                    quoteCount = 0;
                    fieldIdxStart = _buffer.LastReadIdx() + 1;
                }
                else if (c == DOUBLE_QUOTE)
                {
                    if (lastChar == FieldDelimiter)
                    {
                        ++fieldIdxStart;
                        if (ReadQuotedField(out quoteCount) != DOUBLE_QUOTE)
                        {
                            throw new Exception("quoted field has no end quote");
                        }
                        lastChar = DOUBLE_QUOTE;
                    }
                    else
                    {
                        throw new Exception("quotes are not allowed within unquoted fields");
                    }
                }

                lastChar = c;
            }
            //EOF                
            AddField(fieldIdxStart, _buffer.LastReadIdx(), lastChar == DOUBLE_QUOTE, quoteCount);
        }

        private char? ReadQuotedField(out int quoteCount)
        {
            quoteCount = 0;

            char c = '\0';
            char? lastChar = null;

            while ( _buffer.Read(ref c) )
            {
                if ( lastChar == DOUBLE_QUOTE )
                {
                    if ( c == DOUBLE_QUOTE )
                    {
                        ++quoteCount;
                    }
                }
                lastChar = c;
            }
            
            return lastChar;
        }
        private void AddField(int startIdx, int endIdx, bool isQuotedField, int quoteCount)
        {
            int truncate = isQuotedField ? 1 : 0;

            AddFieldToArray(
                startIdx,
                len: (endIdx - startIdx) - truncate,
                quoteCount);
        }
        private void AddFieldToArray(int startIdx, int len, int quoteCount)
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

                    var fieldValue = _buffer.GetSpan(f.startIdx, f.len);

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

                return _buffer.GetSpan(f.startIdx, f.len);
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
