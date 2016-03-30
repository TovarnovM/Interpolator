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
    public class AeroGraphsTests
    {
        private AeroGraphs sample;
        [TestMethod(), TestInitialize()]
        public void AeroGraphsTest()
        {
            sample = new AeroGraphs();
        }

        [TestMethod()]
        public void GetParamsTest()
        {
            Assert.AreEqual(0.83, sample.GetV("3_22", 3, 0.3), 0.1);
        }


        [TestMethod()]
        public void CutMyStringTest()
        {
            Assert.AreEqual("4_5_5", sample.CutMyString("__4_5_5_4D"));
            Assert.AreEqual("4_5_5", sample.CutMyString("4_5_5_4D"));
            Assert.AreEqual("4_5_5", sample.CutMyString("4_5_5"));
        }

        [TestMethod()]
        public void HowManyParamsTest()
        {

        }

        [TestMethod()]
        public void HasGraphTest()
        {

        }

        [TestMethod()]
        public void GetVTest()
        {

        }
    }
}