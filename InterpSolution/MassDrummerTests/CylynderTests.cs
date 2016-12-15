using Microsoft.VisualStudio.TestTools.UnitTesting;
using MassDrummer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace MassDrummer.Tests {
    [TestClass()]
    public class CylynderTests {

        [TestMethod()]
        public void CylynderTest1() {
            var c = new Cylynder();
            c.D = 7;
            c.H = 17;
            c.M = 29;

            var ro = c.FindParam("Ro");
            var ro_ideal = 29/(PI * 7 * 7 * 0.25 * 17);
            Assert.AreEqual(ro_ideal,ro,c.eps);
        }

        [TestMethod()]
        public void CylynderTest2() {
            var c = new Cylynder();
            c.D = 7;
            c.H = 17;
            c.M = 29;
            c.Ro = 29 / (PI * 7 * 7 * 0.25 * 17);

            c.H = 77; 
            var sear = c.FindParam("H");
            var sear_ideal = 17;
            Assert.AreEqual(sear_ideal,sear,0.01);
        }

        [TestMethod()]
        public void CylynderTest3() {
            var c = new Cylynder();
            c.D = 7;
            c.H = 17;
            c.M = 29;
            c.Ro = 29 / (PI * 7 * 7 * 0.25 * 17);

            c.H = 17;
            var sear = c.FindParam("H");
            var sear_ideal = 17;
            Assert.AreEqual(sear_ideal,sear,0.01);
        }

        [TestMethod()]
        public void CylynderTest4() {
            var c = new Cylynder();
            c.D = 7;
            c.H = 17;
            c.M = 29;
            c.Ro = 29 / (PI * 7 * 7 * 0.25 * 17);

            c.H = 0.1;
            var sear = c.FindParam("H");
            var sear_ideal = 17;
            Assert.AreEqual(sear_ideal,sear,0.01);
        }

        [TestMethod()]
        public void CylynderTest5() {
            var c = new Cylynder();
            c.D = 7;
            c.H = 17;
            c.M = 29;
            c.Ro = 29 / (PI * 7 * 7 * 0.25 * 17);

            c.D = 0.1;
            var sear = c.FindParam("D");
            var sear_ideal = 7;
            Assert.AreEqual(sear_ideal,sear,0.01);
        }
    }
}