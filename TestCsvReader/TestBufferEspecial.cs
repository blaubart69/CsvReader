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
        [Fact]
        public void OneCharWithBufsize1()
        {
            var b = new BufferEspecial(new StringReader("a"), buffersize:1);
            char c = '\0';
            Assert.True(b.Read(ref c));
            Assert.Equal('a', c);
            Exception ex = Assert.Throws<Exception>(() =>
            {
                b.Read(ref c);
            });
            Assert.True(ex != null);
        }
        [Fact]
        public void OneCharWithBufsize2()
        {
            var b = new BufferEspecial(new StringReader("a"), buffersize: 2);
            char c = '\0';
            Assert.True(b.Read(ref c));
            Assert.Equal('a', c);
            Assert.False(b.Read(ref c));
        }
    }
}
