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
        readonly Func<BufferEspecial> _bufferFactory;

        readonly char FIELDDELIMITER;

        const int INITIAL_FIELDS_SIZE = 8;
        const char DOUBLE_QUOTE = '\"';

        Field[] _fields;

        int _fieldIdx_toWrite;

        public CsvReader5(TextReader reader, char fieldDelimiter = ',', int buffersize = 4096)
        {
            _bufferFactory = () => new BufferEspecial(reader, buffersize);
            FIELDDELIMITER = fieldDelimiter;
            _fields = new Field[INITIAL_FIELDS_SIZE];
        }
        public bool Read()
        {
            if ( _buffer == null )
            {
                _buffer = _bufferFactory();
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
        private bool ReadOneRecord()
        {
            char c = '\0';
            char lastChar = FIELDDELIMITER;
            int fieldIdxStart = 0;
            int quoteCount = 0;

            if ( !_buffer.ReadNextIdx(ref c))
            {
                return false;
            }

            for (;;)
            {
                if (c == '\n' || c == '\r')
                {
                    if (lastChar != '\n' && lastChar != '\r')
                    {
                        AddField(fieldIdxStart, _buffer.LastReadIdx(), lastChar == DOUBLE_QUOTE, ref quoteCount);
                    }
                    if (c == '\n')
                    {
                        return true;
                    }
                }
                else if (c == FIELDDELIMITER)
                {
                    AddField(fieldIdxStart, _buffer.LastReadIdx(), lastChar == DOUBLE_QUOTE, ref quoteCount);
                    fieldIdxStart = _buffer.LastReadIdx() + 1;
                }
                else if (c == DOUBLE_QUOTE)
                {
                    if (lastChar == FIELDDELIMITER)
                    {
                        ++fieldIdxStart;
                        lastChar = DOUBLE_QUOTE;
                        if ( ReadQuotedField(out quoteCount, ref c))
                        {
                            // field ended normal at a double quote
                            // skip reading char. "c" is already the NEXT char
                            continue; 
                        }
                        else
                        {
                            break; // field ended at EOF
                        }
                    }
                    else
                    {
                        throw new Exception("quotes are not allowed within unquoted fields");
                    }
                }

                lastChar = c;

                if (!_buffer.ReadNextIdx(ref c))
                {
                    break;
                }
            }
            //EOF                
            AddField(fieldIdxStart, _buffer.LastReadIdx(), lastChar == DOUBLE_QUOTE, ref quoteCount);

            return true;
        }

        private bool ReadQuotedField(out int quoteCount, ref char c)
        {
            quoteCount = 0;

            while (_buffer.ReadNextIdx(ref c))
            {
                if (c == DOUBLE_QUOTE)
                {
                    if (_buffer.ReadNextIdx(ref c))
                    {
                        if (c == DOUBLE_QUOTE)
                        {
                            ++quoteCount;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // quoted field ended at EOF
                        return false;
                    }
                }
            }
            throw new Exception("EOF before quoted field was closed");
        }
        private void AddField(int startIdx, int endIdx, bool isQuotedField, ref int quoteCount)
        {
            int truncate = isQuotedField ? 1 : 0;

            AddFieldToArray(
                startIdx,
                len: (endIdx - startIdx) - truncate,
                quoteCount);

            quoteCount = 0;
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
        public void PrintDebugInfo()
        {
            Console.WriteLine("--- fields ---");
            for (int i=0; i<_fieldIdx_toWrite; ++i)
            {
                Console.WriteLine($"field[{i}]\t[{this[i].ToString()}]");
            }
            Console.WriteLine("--- buffer ---");
            _buffer.PrintDebugInfo();
        }
    }
}
