using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Utils.Test
{
    [TestClass]
    public class BitServiceTest
    {
        [TestMethod]
        public void GetBitTest()
        {
            byte a = 0xaa;
            Assert.IsTrue(BitService.GetBit(a, 0) == 0);
            Assert.IsTrue(BitService.GetBit(a, 1) == 1);
            Assert.IsTrue(BitService.GetBit(a, 2) == 0);
            Assert.IsTrue(BitService.GetBit(a, 3) == 1);
            Assert.IsTrue(BitService.GetBit(a, 4) == 0);
            Assert.IsTrue(BitService.GetBit(a, 5) == 1);
            Assert.IsTrue(BitService.GetBit(a, 6) == 0);
            Assert.IsTrue(BitService.GetBit(a, 7) == 1);
        }

        [TestMethod]
        public void SetBitTest()
        {
            byte a = 0;
            a = BitService.SetBit(a, 1);
            a = BitService.SetBit(a, 3);
            a = BitService.SetBit(a, 5);
            a = BitService.SetBit(a, 7);

            Assert.IsTrue(a == 0xaa);
        }

        [TestMethod]
        public void ClearBitTest()
        {
            byte a = 0xaa;
            a = BitService.ClearBit(a, 1);
            a = BitService.ClearBit(a, 3);
            a = BitService.ClearBit(a, 5);
            a = BitService.ClearBit(a, 7);

            Assert.IsTrue(a == 0x00);
        }
    }
}
