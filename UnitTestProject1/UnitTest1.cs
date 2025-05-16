using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            int expectedResult = 3;
            int actualResult = 1 + 1;
            Assert.AreEqual(expectedResult, actualResult, "1 + 1 should equal 2");
        }
    }
}
