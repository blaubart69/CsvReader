using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCsvReader
{
    /// <summary>
    /// Summary description for TestSilly
    /// </summary>
    [TestClass]
    public class TestArrayCopy
    {
        [TestMethod]
        public void CopyToSameArray()
        {
            char[] buf = "Bernhard".ToCharArray();

            Array.Copy(
                sourceArray: buf,
                sourceIndex: 4,
                destinationArray: buf,
                destinationIndex: 0,
                length: 4);

            CollectionAssert.AreEqual("hardhard".ToCharArray(), buf);
            Assert.AreEqual(          "hardhard", new string(buf));

        }
    }
}
