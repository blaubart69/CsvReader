using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spi;

namespace TestCsvReader
{
    [TestClass]
    public class TestCsvRead
    {
        [TestMethod]
        public void one_lonely_field()
        {
            var actual = RunCsvReader(new StringReader("a"));
            var expected = new string[][] { new string[] { "a" } };
            CompareGrids(expected, actual);
        }
        [TestMethod]
        public void one_lonely_field_crlf()
        {
            var actual = RunCsvReader(new StringReader("a\r\n"));
            var expected = new string[][] { new string[] { "a" } };
            CompareGrids(expected, actual);
        }
        [TestMethod]
        public void one_lonely_field_lf()
        {
            var actual = RunCsvReader(new StringReader("a\n"));
            var expected = new string[][] { new string[] { "a" } };
            CompareGrids(expected, actual);
        }
        [TestMethod]
        public void Line1_Field2()
        {
            var actual = RunCsvReader(new StringReader("a,b"));
            var expected = new string[][] { new string[] { "a", "b" } };
            CompareGrids(expected, actual);
        }
        [TestMethod]
        public void Line1_Field2_ending_with_CrLf()
        {
            var actual = RunCsvReader(new StringReader("a,b\r\n"));
            var expected = new string[][] { new string[] { "a", "b" } };
            CompareGrids(expected, actual);
        }
        [TestMethod]
        public void Line2_Field2()
        {
            var actual = RunCsvReader(new StringReader("a,b\r\nc,d"));
            var expected = new string[][] { 
                new string[] { "a", "b" },
                new string[] { "c", "d" } };
            CompareGrids(expected, actual);
        }
        [TestMethod]
        public void Line1_Field2_quoted()
        {
            CompareGrids(new string[][] { new string[] { "a", "b" } }, RunCsvReader(new StringReader("\"a\",\"b\"")));
        }
        [TestMethod]
        public void twoEmptyQuotedFields()
        {
            CompareGrids(new string[][] { new string[] { "", "" } } , RunCsvReader(new StringReader("\"\",\"\"")));
        }
        [TestMethod]
        public void twoEmptyQuotedFields_CrLf_ending()
        {
            CompareGrids(new string[][] { new string[] { "", "" } }, RunCsvReader(new StringReader("\"\",\"\"\r\n")));
        }
        [TestMethod]
        public void twoQuotedDoubleQuotes()
        {
            CompareGrids(new string[][] { new string[] { "\"", "\"" } }, RunCsvReader(new StringReader("\"\"\"\",\"\"\"\"")));
        }
        [TestMethod]
        public void twoQuotedDoubleQuotes_literal()
        {
            CompareGrids(new string[][] { new string[] { "\"", "\"" } }, RunCsvReader(new StringReader(@""""""""",""""""""")));
        }
        [TestMethod]
        public void twoQuotedDoubleQuotes_crlf()
        {
            CompareGrids(new string[][] { new string[] { "\"", "\"" } }, RunCsvReader(new StringReader("\"\"\"\",\"\"\"\"\r\n")));
        }
        [TestMethod]
        public void emptyFieldInbetween()
        {
            CompareGrids(new string[][] { new string[] { "a", "", "c" } }, RunCsvReader(new StringReader("a,,c")));
        }
        [TestMethod]
        public void emptyQuotedFieldInbetween()
        {
            CompareGrids(new string[][] { new string[] { "a", "", "c" } }, RunCsvReader(new StringReader("a,\"\",c")));
        }
        [TestMethod]
        public void TwoEmptyFields()
        {
            CompareGrids(new string[][] { new string[] { "", "" } }, RunCsvReader(new StringReader(",")));
        }
        [TestMethod]
        public void OneEmptyField()
        {
            CompareGrids(new string[][] { }, RunCsvReader(new StringReader("")));
        }
        [TestMethod]
        public void TwoEmptyFieldsInTwoLines()
        {
            CompareGrids(new string[][] { 
                new string[] { "", "" }, 
                new string[] { "", "" } }
            , RunCsvReader(new StringReader(",\r\n,")));
        }
        [TestMethod]
        public void QuoteInTheMiddle()
        {
            CompareGrids(new string[][] { new string[] { "a", "bb", "c" } }, RunCsvReader(new StringReader("a,b\"b,c")));
        }
        [TestMethod]
        public void QuotedWord()
        {
            CompareGrids(new string[][] { new string[] { "Bernhard \"Florian\" Spindler" } }, 
                RunCsvReader(new StringReader("\"Bernhard \"\"Florian\"\" Spindler\"")));
        }
        [TestMethod]
        public void QuotedDelimiter()
        {
            CompareGrids(new string[][] { new string[] { "Bernhard, \"Florian\", Spindler" } },
                RunCsvReader(new StringReader("\"Bernhard, \"\"Florian\"\", Spindler\"")));
        }
        [TestMethod]
        public void QuotedFieldDelimiter()
        {
            CompareGrids(new string[][] { new string[] { "Bernhard,Florian,Spindler" } },
                RunCsvReader(new StringReader("\"Bernhard,Florian,Spindler\"")));
        }
        [TestMethod]
        public void QuotedFieldDelimiterTwoFields()
        {
            CompareGrids(new string[][] { new string[] { "Bernhard,Florian,Spindler", "Präßler" } },
                RunCsvReader(new StringReader("\"Bernhard,Florian,Spindler\",Präßler")));
        }
        [TestMethod]
        public void OnlyOneQuoteBetweenQuotes()
        {
            CompareGrids(new string[][] { new string[] { "" } },
                RunCsvReader(new StringReader("\"\"\"")));
        }
        #region HELPER
        private List<string[]> RunCsvReader(TextReader r)
        {
            List<string[]> data = new List<string[]>();

            using (r)
            {
                Spi.CsvReader.Run(r, ',', row => data.Add(row) );
            }

            return data;
        }
        private void CompareGrids(string[][] expected, List<string[]> actual)
        {
            Assert.AreEqual(expected.Length, actual.Count, "different row count");

            for ( int i=0; i < expected.Length; ++i)
            {
                for ( int j=0; j < expected[i].Length; ++j)
                {
                    Assert.AreEqual(expected[i][j], actual[i][j]);
                }
            }
        }
        #endregion
    }
}
