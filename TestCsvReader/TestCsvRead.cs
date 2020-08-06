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
            var data = RunCsvReader(new StringReader("a"));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(1, data[0].Length);
            Assert.AreEqual("a", data[0][0]);
        }
        [TestMethod]
        public void one_lonely_field_crlf()
        {
            var data = RunCsvReader(new StringReader("a\r\n"));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(1, data[0].Length);
            Assert.AreEqual("a", data[0][0]);
        }
        [TestMethod]
        public void one_lonely_field_lf()
        {
            var data = RunCsvReader(new StringReader("a\n"));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(1, data[0].Length);
            Assert.AreEqual("a", data[0][0]);
        }
        [TestMethod]
        public void Line1_Field2()
        {
            var data = RunCsvReader(new StringReader("a,b"));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(2, data[0].Length);
            Assert.AreEqual("a", data[0][0]);
            Assert.AreEqual("b", data[0][1]);
        }
        [TestMethod]
        public void Line1_Field2_ending_with_CrLf()
        {
            var data = RunCsvReader(new StringReader("a,b\r\n"));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(2, data[0].Length);
            Assert.AreEqual("a", data[0][0]);
            Assert.AreEqual("b", data[0][1]);
        }
        [TestMethod]
        public void Line2_Field2()
        {
            var data = RunCsvReader(new StringReader("a,b\r\nc,d"));
            Assert.AreEqual(2, data.Count);
            Assert.AreEqual(2, data[0].Length);
            Assert.AreEqual(2, data[1].Length);
            Assert.AreEqual("a", data[0][0]);
            Assert.AreEqual("b", data[0][1]);
            Assert.AreEqual("c", data[1][0]);
            Assert.AreEqual("d", data[1][1]);
        }
        [TestMethod]
        public void Line1_Field2_quoted()
        {
            var data = RunCsvReader(new StringReader("\"a\",\"b\""));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(2, data[0].Length);
            Assert.AreEqual("a", data[0][0]);
            Assert.AreEqual("b", data[0][1]);
        }
        [TestMethod]
        public void twoEmptyQuotedFields()
        {
            var data = RunCsvReader(new StringReader("\"\",\"\""));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(2, data[0].Length);
            Assert.AreEqual("", data[0][0]);
            Assert.AreEqual("", data[0][1]);
        }
        [TestMethod]
        public void twoEmptyQuotedFields_CrLf_ending()
        {
            var data = RunCsvReader(new StringReader("\"\",\"\"\r\n"));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(2, data[0].Length);
            Assert.AreEqual("", data[0][0]);
            Assert.AreEqual("", data[0][1]);
        }
        [TestMethod]
        public void twoQuotedDoubleQuotes()
        {
            var data = RunCsvReader(new StringReader("\"\"\"\",\"\"\"\""));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(2, data[0].Length);
            Assert.AreEqual("\"", data[0][0]);
            Assert.AreEqual("\"", data[0][1]);
        }
        [TestMethod]
        public void twoQuotedDoubleQuotes_literal()
        {
            var data = RunCsvReader(new StringReader(@""""""""","""""""""));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(2, data[0].Length);
            Assert.AreEqual("\"", data[0][0]);
            Assert.AreEqual("\"", data[0][1]);
        }
        [TestMethod]
        public void twoQuotedDoubleQuotes_crlf()
        {
            var data = RunCsvReader(new StringReader("\"\"\"\",\"\"\"\"\r\n"));
            Assert.AreEqual(1, data.Count);
            Assert.AreEqual(2, data[0].Length);
            Assert.AreEqual("\"", data[0][0]);
            Assert.AreEqual("\"", data[0][1]);
        }
        private List<string[]> RunCsvReader(TextReader r)
        {
            List<string[]> data = new List<string[]>();

            using (r)
            {
                Spi.CsvReader.Run(r, ',', row => data.Add(row) );
            }

            return data;
        }
    }
}
