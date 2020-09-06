using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace TestCsvReader4
{
    public class TestCsvReader5
    {
        [Fact]
        public void one_lonely_field()
        {
            var actual = RunCsvReader(new StringReader("a"));
            var expected = new string[][] { new string[] { "a" } };
            CompareGrids(expected, actual);
        }
        [Fact]
        public void one_lonely_field_crlf()
        {
            var actual = RunCsvReader(new StringReader("a\r\n"));
            var expected = new string[][] { new string[] { "a" } };
            CompareGrids(expected, actual);
        }
        [Fact]
        public void one_lonely_field_lf()
        {
            var actual = RunCsvReader(new StringReader("a\n"));
            var expected = new string[][] { new string[] { "a" } };
            CompareGrids(expected, actual);
        }
        [Fact]
        public void Line1_Field2()
        {
            var actual = RunCsvReader(new StringReader("a,b"));
            var expected = new string[][] { new string[] { "a", "b" } };
            CompareGrids(expected, actual);
        }
        [Fact]
        public void Line1_Field2_ending_with_CrLf()
        {
            var actual = RunCsvReader(new StringReader("a,b\r\n"));
            var expected = new string[][] { new string[] { "a", "b" } };
            CompareGrids(expected, actual);
        }
        [Fact]
        public void Line2_Field2()
        {
            var actual = RunCsvReader(new StringReader("a,b\r\nc,d"));
            var expected = new string[][] {
                new string[] { "a", "b" },
                new string[] { "c", "d" } };
            CompareGrids(expected, actual);
        }
        [Fact]
        public void Line1_Field2_quoted()
        {
            CompareGrids(new string[][] { new string[] { "a", "b" } }, RunCsvReader(new StringReader("\"a\",\"b\"")));
        }
        [Fact]
        public void twoEmptyQuotedFields()
        {
            CompareGrids(new string[][] { new string[] { "", "" } }, RunCsvReader(new StringReader("\"\",\"\"")));
        }
        [Fact]
        public void twoEmptyQuotedFields_CrLf_ending()
        {
            CompareGrids(new string[][] { new string[] { "", "" } }, RunCsvReader(new StringReader("\"\",\"\"\r\n")));
        }
        [Fact]
        public void twoQuotedDoubleQuotes()
        {
            CompareGrids(new string[][] { new string[] { "\"", "\"" } }, RunCsvReader(new StringReader("\"\"\"\",\"\"\"\"")));
        }
        [Fact]
        public void twoQuotedDoubleQuotes_literal()
        {
            CompareGrids(new string[][] { new string[] { "\"", "\"" } }, RunCsvReader(new StringReader(@""""""""",""""""""")));
        }
        [Fact]
        public void twoQuotedDoubleQuotes_crlf()
        {
            CompareGrids(new string[][] { new string[] { "\"", "\"" } }, RunCsvReader(new StringReader("\"\"\"\",\"\"\"\"\r\n")));
        }
        [Fact]
        public void emptyFieldInbetween()
        {
            CompareGrids(new string[][] { new string[] { "a", "", "c" } }, RunCsvReader(new StringReader("a,,c")));
        }
        [Fact]
        public void emptyQuotedFieldInbetween()
        {
            CompareGrids(new string[][] { new string[] { "a", "", "c" } }, RunCsvReader(new StringReader("a,\"\",c")));
        }
        [Fact]
        public void TwoEmptyFields()
        {
            CompareGrids(new string[][] { new string[] { "", "" } }, RunCsvReader(new StringReader(",")));
        }
        [Fact]
        public void OneEmptyField()
        {
            CompareGrids(new string[][] { }, RunCsvReader(new StringReader("")));
        }
        [Fact]
        public void TwoEmptyFieldsInTwoLines()
        {
            CompareGrids(new string[][] {
                new string[] { "", "" },
                new string[] { "", "" } }
            , RunCsvReader(new StringReader(",\r\n,")));
        }
        [Fact]
        public void QuoteInTheMiddle()
        {
            Exception ex = Assert.Throws<Exception>(() =>
            {
                CompareGrids(new string[][] { new string[] { "a", "bb", "c" } }, RunCsvReader(new StringReader("a,b\"b,c")));
            });
            Assert.Equal("quotes are not allowed within unquoted fields", ex.Message);
        }
        [Fact]
        public void QuotedWord()
        {
            CompareGrids(new string[][] { new string[] { "Bernhard \"Florian\" Spindler" } },
                RunCsvReader(new StringReader("\"Bernhard \"\"Florian\"\" Spindler\"")));
        }
        [Fact]
        public void QuotedDelimiter()
        {
            CompareGrids(new string[][] { new string[] { "Bernhard, \"Florian\", Spindler" } },
                RunCsvReader(new StringReader("\"Bernhard, \"\"Florian\"\", Spindler\"")));
        }
        [Fact]
        public void QuotedFieldDelimiter()
        {
            CompareGrids(new string[][] { new string[] { "Bernhard,Florian,Spindler" } },
                RunCsvReader(new StringReader("\"Bernhard,Florian,Spindler\"")));
        }
        [Fact]
        public void QuotedFieldDelimiterTwoFields()
        {
            CompareGrids(new string[][] { new string[] { "Bernhard,Florian,Spindler", "Pr‰ﬂler" } },
                RunCsvReader(new StringReader("\"Bernhard,Florian,Spindler\",Pr‰ﬂler")));
        }
        [Fact]
        public void OnlyOneQuoteBetweenQuotes()
        {
            Exception ex = Assert.Throws<Exception>(() =>
            {
                CompareGrids(new string[][] { new string[] { "" } },
                RunCsvReader(new StringReader("\"\"\"")));
            });
            Assert.Equal("missing end quote", ex.Message);
        }
        [Fact]
        public void RealLife1()
        {
                                                      // "unanimously approved";"No";"";"";"";"No date entered."
            CompareGrids(new string[][] { new string[] { "unanimously approved", "No","","","", "No date entered." } },
                         RunCsvReader(new StringReader("\"unanimously approved\";\"No\";\"\";\"\";\"\";\"No date entered.\""),delim: ';'));
        }
        [Fact]
        public void RealLife2()
        {
            string test = @"""------------------"";""--------------------------------------------------"";"""";""----"";""----------------------------------------------"";""-------"";""--"";"""";""--------"";""-------"";""----------"";"""";""-------------------------------------------------------------------------------
-------------------"";""--------------------"";""No"";"""";"""";"""";""No date entered."";"""";"""";"""";"""";""0"";""0"";""""";
  
            // "unanimously approved";"No";"";"";"";"No date entered."
            CompareGrids(new string[][] { new string[] {
                "------------------",
                "--------------------------------------------------",
                "",
                "----",
                "----------------------------------------------",
                "-------",
                "--",
                "",
                "--------",
                "-------",
                "----------",
                "",
                "-------------------------------------------------------------------------------\r\n-------------------",
                "--------------------",
                "No",
                "",
                "",
                "",
                "No date entered.",
                "",
                "",
                "",
                "",
                "0",
                "0",
                ""
                } },
                         RunCsvReader(new StringReader(test), delim: ';'));
        }

        [Fact]
        public void BufferEdges()
        {
            CompareGrids(new string[][] { new string[] { "2345" }, new string[] { "90" } },
                         RunCsvReader(new StringReader("\"2345\"\r\n\"90\""), buffersize: 10));
        }
        [Fact]
        public void BufferEndsWithAndDoubleQuote()
        {
            CompareGrids(new string[][] { new string[] { "123","567" }, new string[] { "90", "" } },
                         RunCsvReader(new StringReader("123,567\r\n\"90\",\"\""), buffersize: 10));
        }
        [Fact]
        public void DoubleQuoteIsFirstInShiftedBuffer()
        {
            CompareGrids(new string[][] { new string[] { "123", "5678" }, new string[] { "90", "" } },
                         RunCsvReader(new StringReader("123,5678\r\n\"90\",\"\""), buffersize: 10));
        }
        [Fact]
        public void BufferEmptyFields1()
        {
            CompareGrids(new string[][] { new string[] { "12", "34" }, new string[] { "", "", "" } },
                         RunCsvReader(new StringReader("12,34\r\n\"\",\"\",\"\""), buffersize: 10));
        }
        #region HELPER
        private List<string[]> RunCsvReader(TextReader r, int buffersize=1024, char delim=',')
        {
            List<string[]> data = new List<string[]>();

            using (r)
            {
                var csvrdr = new Spi.CsvReader5(r, delim, buffersize);
                while (csvrdr.Read())
                {
                    List<string> row = new List<string>();
                    for (int i=0; i<csvrdr.FieldCount; ++i)
                    {
                        row.Add(csvrdr[i].ToString());
                    }
                    data.Add(row.ToArray());
                }
            }

            return data;
        }
        private void CompareGrids(string[][] expected, List<string[]> actual)
        {
            Assert.Equal(expected.Length, actual.Count);

            for (int i = 0; i < expected.Length; ++i)
            {
                for (int j = 0; j < expected[i].Length; ++j)
                {
                    Assert.Equal(expected[i][j], actual[i][j]);
                }
            }
        }
        #endregion

    }
}
