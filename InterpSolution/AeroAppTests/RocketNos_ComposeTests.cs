using Microsoft.VisualStudio.TestTools.UnitTesting;
using RocketAero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketAero.Tests
{
    [TestClass()]
    public class RocketNos_ComposeTests
    {
        private RocketNos_Compose Nose72, Nose71, Nose82, Nose81;
        [TestMethod()]
        public void getLmbdShtrTest()
        {
            Assert.AreEqual(1.571, Nose72.GetLmbdShtr(1.148387),0.001);
            Assert.AreEqual(3.335, Nose71.GetLmbdShtr(1.148387), 0.001);
            Assert.AreEqual(2.35, Nose82.GetLmbdShtr(1.148387), 0.001);
            Assert.AreEqual(4.8108, Nose81.GetLmbdShtr(1.148387), 0.001);
        }

        [TestMethod(), TestInitialize()]
        public void RocketNos_ComposeTest()
        {
            Nose72 = new RocketNos_Compose("7_2", 118 * 2 / 310.0);
            Nose71 = new RocketNos_Compose("7_1", 118 * 2 / 310.0);
            Nose82 = new RocketNos_Compose("8_2", 118 * 2 / 310.0);
            Nose81 = new RocketNos_Compose("8_1", 118 * 2 / 310.0);
        }

        [TestMethod()]
        public void GetF_nosTest()
        {
            Assert.AreEqual(0.28, Nose72.GetF_nos(0.31,0.356), 0.001);
            Assert.AreEqual(0.308, Nose71.GetF_nos(0.31, 0.356), 0.001);
            Assert.AreEqual(0.322, Nose82.GetF_nos(0.31, 0.356), 0.001);
            Assert.AreEqual(0.3574, Nose81.GetF_nos(0.31, 0.356), 0.001);
        }


        [TestMethod()]
        public void GetW_nosTest()
        {
            Assert.AreEqual(0.01714, Nose72.GetW_nos(0.31, 0.356), 0.001);
            Assert.AreEqual(17592452.522122 / 1000000000.0, Nose71.GetW_nos(0.31, 0.356), 0.001);
            Assert.AreEqual(0.02062, Nose82.GetW_nos(0.31, 0.356), 0.001);
            Assert.AreEqual(20966030.523690 / 1000000000.0, Nose81.GetW_nos(0.31, 0.356), 0.001);
        }
    }
}