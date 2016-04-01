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
    public class RocketBodyTests
    {

        [TestMethod(), TestInitialize()]
        public void RocketBodyTest()
        {
            AG = new AeroGraphs();
            var RB = new RocketBody(AG)
            {
                Nose = new RocketNos_Compose("7_2", 0.3),
                L = 2,
                D = 0.2,
                L_nos = 0.4,
                L_korm = 0.2,
                D1 = 0.3
            };
            Assert.AreEqual(2, RB.Lmb_nos);
            Assert.AreEqual(7, RB.Lmb_cyl);
            Assert.AreEqual(1, RB.Lmb_korm);
        }

        private AeroGraphs AG;
        [TestMethod()]
        public void GetCy1aTest()
        {
            var RB = new RocketBody(AG)
            {
                Nose = new RocketNos_Compose("7_2", 0.3),
                L = 2,
                D = 0.2,
                L_nos = 0.4,
                L_korm = 0.2,
                D1 = 0.3
            };
            double mach = 2;
            Assert.AreEqual(0.055, RB.AeroGr.GetV("3_2", 0.86, 3.5), 0.002);
            Assert.AreEqual(0.043, RB.AeroGr.GetV("3_4", 0.24, 1), 0.002);
            Assert.AreEqual(0.054, RB.Nose.GetCy1a_nos(AG, mach, 2, 7),0.001);
            Assert.AreEqual(0.0848, RB.GetCy1a(mach), 0.002);

            RB.D1 = 0.1;
            Assert.AreEqual(0.049, RB.GetCy1a(mach), 0.002);

            RB.L = 1;
            RB.Nose = new RocketNos_OzjPlusCyl();
            mach = 3;
            Assert.AreEqual(0.048, RB.Nose.GetCy1a_nos(AG, mach, 2, 2), 0.001);
            Assert.AreEqual(0.043, RB.GetCy1a(mach), 0.001);
        }

    }
}