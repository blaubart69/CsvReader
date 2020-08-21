using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace NReco.Csv.Tests {

	public class CsvReaderTests {

		private readonly ITestOutputHelper output;

		public CsvReaderTests(ITestOutputHelper output) {
			this.output = output;
		}

		string sampleCsv = 
@"A,B,C
1,5.5,5 Jun 2014
2,0,6/6/2014
3,4.6,6-7-2014
4,2,6-5-2014";

		[Fact]
		public void ParseCsvTest() {
			var csvStream = new MemoryStream( System.Text.Encoding.UTF8.GetBytes(sampleCsv) );

			var csvReader = new Spi.CsvReader3(new StreamReader(csvStream), ',');
			int sumA = 0;
			decimal sumB = 0;
			int line = 0;
			while (csvReader.Read()) {
				line++;
				if (line==1) {
					continue; // skip header row
				}
				//sumA += Convert.ToInt32(csvReader[0], CultureInfo.InvariantCulture);
				//sumB += Convert.ToDecimal(csvReader[1], CultureInfo.InvariantCulture);
				sumA += Int32  .Parse(csvReader[0], provider: CultureInfo.InvariantCulture);
				sumB += Decimal.Parse(csvReader[1], provider: CultureInfo.InvariantCulture);
			}
			Assert.Equal(5, line);
			Assert.Equal(10, sumA);
			Assert.Equal(12.1M, sumB);
		}

		string strangeHdrSampleCsv = "\"A\nA\",B  B,\"C\r\n\tC\"\n1,5.5,5 Jun 2014";

		[Fact]
		public void StrangeHeadersTest() {
			var csvReader = new Spi.CsvReader3(new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(strangeHdrSampleCsv))), ',');
			Assert.True(csvReader.Read());

			Assert.Equal("A\nA",		csvReader[0].ToString());
			Assert.Equal("B  B",		csvReader[1].ToString());
			Assert.Equal("C\r\n\tC",	csvReader[2].ToString());
			Assert.True(csvReader.Read());
			Assert.Equal("1",			csvReader[0].ToString());
			Assert.Equal("5.5",			csvReader[1].ToString());
			Assert.Equal("5 Jun 2014",	csvReader[2].ToString());
		}
		/*
		[Fact]
		public void CsvOptionsTest() {
			var tests = new string[] {
				// tab
				"A\tB\tC \r\n1\t2\t 3\r\n\r\n5\t\t\n\"6\"\t \"7\"\"\" \t\"\"\"\"",
				// custom 3-symobls
				"A%%% \"B\"%%%C\n 1%%%2%%%3 \n5%%%6%%6%%%7%\n",
				// no trim
				"A,B,C\n1 , 2  ,3 \n  4,5, 6\n \"7\",\"\"8 ,\"9\"",
				"A,B,C",
				"A,B,C\n1,2,3"
			};
			var testDelims = new[] { "\t", "%%%", ",", ",", "," };
			var testTrimFields = new[] { true, true, false, true, true };
			var testBufSize = new[] { 1024, 1024, 1024, 5, 5};
			var expected = new string[] {
				"1|2|3|#5|||#6|7\"|\"|#",
				"1|2|3|#5|6%%6|7%|#",
				"1 | 2  |3 |#  4|5| 6|# \"7\"|\"\"8 |9|#",
				"",
				"1|2|3|#"
			};
			for (int i=0; i<tests.Length; i++) {
				var csvRdr = new CsvReader(
					new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(tests[i]))),
					testDelims[i]);
				csvRdr.TrimFields = testTrimFields[i];
				csvRdr.BufferSize = testBufSize[i];

				var sb = new StringBuilder();
				csvRdr.Read(); // skip header row
				while (csvRdr.Read()) {
					sb.Append(csvRdr[0] + "|");
					sb.Append(csvRdr[1] + "|");
					sb.Append(csvRdr[2] + "|");
					sb.Append("#");
				}
				Assert.Equal(expected[i], sb.ToString());
			}
		}
		*/
		[Fact]
		public void LongLineCsvTest() {
			var test = "ABCDEF,123456";
			var csvRdr = new Spi.CsvReader3(new StreamReader(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(test))),',', buffersize: 5);
			Assert.Throws<Exception>(() => {
				csvRdr.Read();
			});
		}
		
		[Fact]
		public void LargeCsvTest() {
			var memStream = new MemoryStream();
			int UpperLimit = 50_000_000;  // note: in Debug mode processing takes much more time in comparing to Release!

			int records_written = 0;
			using (var streamWr = new StreamWriter(memStream, Encoding.UTF8, 4096, leaveOpen: true)) {
				var bVals = new string[] { "b1", "b2", "b3" };
				for (int i=0; i< UpperLimit; i+=100) {
					//streamWr.WriteLine(String.Format("{0},{1}, \"Quoted\", just a value", i, bVals[i%bVals.Length] ));
					  streamWr.WriteLine(String.Format("{0},{1},\"Quoted\", just a value", i, bVals[i % bVals.Length]));
					++records_written;
				}
			}
			memStream.Position = 0;

			output.WriteLine("CSV len = " + memStream.Length.ToString());
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();

			var csvRdr = new Spi.CsvReader3(new StreamReader(memStream), buffersize: 64);
			int record_read = 0;
			while (csvRdr.Read()) {
				if (csvRdr[1][0] != 'b')
				{
					throw new Exception("Wrong read!");
				}
				output.WriteLine($"[{csvRdr[0].ToString()}][{csvRdr[1].ToString()}][{csvRdr[2].ToString()}][{csvRdr[3].ToString()}]");
				++record_read;
			}

			sw.Stop();
			output.WriteLine("Time: {0}ms", sw.ElapsedMilliseconds);

			Assert.Equal(records_written, record_read);
		}
		/*
		[Fact]
		public void ProcessValueInBufferTest() {
			var sb = new StringBuilder();
			for (int i=0; i<10000; i++) {
				sb.AppendLine("Some test value, \"Some value with \"\"quotes\"\"\",\"Simple in quotes\",a ");
			}
			var csvRdr = new CsvReader(new StringReader(sb.ToString())) { BufferSize = 100 };
			while (csvRdr.Read()) {
				csvRdr.ProcessValueInBuffer(0, (buf, start, len) => {
					Assert.Equal("Some test value", new string(buf, start, len));
				});
				csvRdr.ProcessValueInBuffer(1, (buf, start, len) => {
					Assert.Equal("Some value with \"quotes\"", new string(buf, start, len));
				});
				csvRdr.ProcessValueInBuffer(2, (buf, start, len) => {
					Assert.Equal("Simple in quotes", new string(buf, start, len));
				});
				csvRdr.ProcessValueInBuffer(3, (buf, start, len) => {
					Assert.Equal("a", new string(buf, start, len));
				});
			}
		}
		*/
	}
}
