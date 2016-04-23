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
    public class RocketWingTests
    {
        private AeroGraphs AG;
        private RocketWing RW;
        [TestMethod()]
        public void WingGeometryTest()
        {
            Assert.AreEqual(26.3, RW.B_b, 0.02);
            Assert.AreEqual(25.2, RW.B_a, 0.02);
            Assert.AreEqual(19.06, RW.B_ak, 0.02);
            Assert.AreEqual(Math.Tan(18*Math.PI/180), RW.GetTgHiM(0.5), 0.02);
            Assert.AreEqual(22.27, RW.Z_a, 0.02);
            Assert.AreEqual(34.93, RW.Z_ak, 0.02);
            Assert.AreEqual(11.33, RW.X_b, 0.02);
            Assert.AreEqual(19.79, RW.X_ak, 0.04);
            Assert.AreEqual(2 * 629.812233, RW.S_k, 2);
            Assert.AreEqual(2 * 1252.83194, RW.S, 2);
            Assert.AreEqual(4, RW.Etta, 0.02);
            Assert.AreEqual(26.3/9, RW.Etta_k, 0.02);
            Assert.AreEqual(4.04, RW.Lmb_k, 0.02);
        }
        [TestMethod()]
        public void WingGraffTest()
        {
            Assert.AreEqual(0.215, Math.Pow(RW.C_shtr, 1.0/3), 0.002);
            Assert.AreEqual(0.022* RW.Lmb_k, RW.GetCy1a(1.1),0.001);
        }
        [TestMethod(), TestInitialize()]
        public void RocketWingTest()
        {
            AG = new AeroGraphs();
            RW = new RocketWing(AG)
            {
                D = 40,
                B0 = 36,
                B1 = 9,
                Hi0 = 29.5,
                L = 55.68 * 2,
                C_shtr = 0.01
                
            };
        }
    }
}