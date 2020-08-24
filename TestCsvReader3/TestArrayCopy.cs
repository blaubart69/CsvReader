using System;
using System.Text;
using System.Collections.Generic;
using Xunit;

namespace TestCsvReader
{
    public class TestArrayCopy
    {
        [Fact]
        public void CopyToSameArray()
        {
            char[] buf = "Bernhard".ToCharArray();

            Array.Copy(
                sourceArray: buf,
                sourceIndex: 4,
                destinationArray: buf,
                destinationIndex: 0,
                length: 4);

            Assert.Equal("hardhard", new string(buf));
        }
    }
}
