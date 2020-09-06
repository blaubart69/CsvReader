using Spi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace TestCsvReader
{
    public class TestBufferEspecial
    {
        [Fact]
        public void ReadEmptyStream()
        {
            var b = new BufferEspecial(new StringReader(""));
            char c = '\0';
            Assert.False(b.Read(ref c));



        }

    }
}
