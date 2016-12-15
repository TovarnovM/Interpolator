using Microsoft.VisualStudio.TestTools.UnitTesting;
using MassDrummer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassDrummer.Tests {
    [TestClass()]
    public class ConeConeTests {
        [TestMethod()]
        public void F_ot_xTest1() {
            var c = new ConeCone();
            c.H1 = 7;
            c.H2 = 3;
            c.D = 4;

            Assert.AreEqual(2,c.F_ot_x(7),0.0000001);
        }
        [TestMethod()]
        public void F_ot_xTest2() {
            var c = new ConeCone();
            c.H1 = 7;
            c.H2 = 3;
            c.D = 4;

            Assert.AreEqual(1,c.F_ot_x(3.5),0.0000001);
        }
        [TestMethod()]
        public void F_ot_xTest3() {
            var c = new ConeCone();
            c.H1 = 7;
            c.H2 = 3;
            c.D = 4;

            Assert.AreEqual(1,c.F_ot_x(8.5),0.0000001);
        }
        [TestMethod()]
        public void F_ot_xTest4() {
            var c = new ConeCone();
            c.H1 = 7;
            c.H2 = 3;
            c.D = 4;

            Assert.AreEqual(0,c.F_ot_x(10),0.0000001);
        }

        [TestMethod()]
        public void FindH1() {
            var c = new ConeCone();
            c.H1 = 7;
            c.H2 = 3;
            c.D = 4;
            c.M = 10;

            c.Ro = c.FindParam("Ro");

            c.H1 = 10;

            var answ = c.FindParam("H1");
            Assert.AreEqual(7,answ,0.001);
        }

        [TestMethod()]
        public void FindH2() {
            var c = new ConeCone();
            c.H1 = 7;
            c.H2 = 0;
            c.D = 4;
            c.M = 10;

            c.Ro = c.FindParam("Ro");

            c.H2 = 10;

            var answ = c.FindParam("H2");
            Assert.AreEqual(0,answ,0.001);
        }
        [TestMethod()]
        public void FindH22() {
            var c = new ConeCone();
            c.H1 = 7;
            c.H2 = -5;
            c.D = 4;
            c.M = 10;

            c.Ro = c.FindParam("Ro");

            c.H2 = 10;

            var answ = c.FindParam("H2");
            Assert.AreEqual(-5,answ,0.001);
        }

    }
}