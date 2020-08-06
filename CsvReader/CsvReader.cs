using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Spi
{
    public class CsvReader
    {
        public static void Run(TextReader reader, char fieldDelimiter, Action<string[]> OnRow)
        {
            List<string> fields = new List<string>();
            StringBuilder sb = new StringBuilder();

            bool eof;
            do
            {
                eof = ReadRow(reader, fieldDelimiter, ref fields, ref sb);
                if (fields.Count > 0)
                {
                    OnRow(fields.ToArray());
                }
            }
            while (!eof);
        }

        private const char DOUBLE_QUOTE = '"';
        private static bool ReadRow(TextReader reader, char fieldDelimiter, ref List<string> fields, ref StringBuilder sb)
        {
            sb.Clear();
            fields.Clear();

            int int_c = reader.Read();
            if (int_c == -1)
            {
                return true;
            }

            bool eof = false;
            bool inQuotes = false;
            char? lastChar = fieldDelimiter;

            for (;;)
            {
                bool setLastChar = true;
                char c = (char)int_c;

                // -------------------------------------------------------------------------
                if (c == DOUBLE_QUOTE)
                // -------------------------------------------------------------------------
                {
                    if (lastChar == fieldDelimiter)
                    {
                        inQuotes = true;
                        setLastChar = false;
                    }
                    else if (lastChar == DOUBLE_QUOTE)
                    {
                        sb.Append(DOUBLE_QUOTE);
                        setLastChar = false;
                    }
                }
                // -------------------------------------------------------------------------
                else if (c == fieldDelimiter)
                // -------------------------------------------------------------------------
                {
                    if (lastChar == DOUBLE_QUOTE)
                    {
                        inQuotes = false;
                    }

                    if (inQuotes)
                    {
                        sb.Append(fieldDelimiter);
                    }
                    else
                    {
                        fields.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                // -------------------------------------------------------------------------
                else if (c == '\n' || c == '\r')
                // -------------------------------------------------------------------------
                {
                    if (inQuotes)
                    {
                        if ( lastChar == DOUBLE_QUOTE )
                        {
                            inQuotes = false;
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                    if ( !inQuotes && c == '\n')
                    {
                        fields.Add(sb.ToString());
                        break;
                    }
                }
                // -------------------------------------------------------------------------
                else
                // -------------------------------------------------------------------------
                {
                    sb.Append(c);
                }

                lastChar = setLastChar ? c : (char?)null;

                int_c = reader.Read();
                if (int_c == -1)
                {
                    eof = true;
                    fields.Add(sb.ToString());
                    break;
                }
            }

            return eof;
        }
    }
}
