using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeetingPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro.Tests {
    [TestClass()]
    public class MisTests {
        Mis mis;

        [TestMethod(), TestInitialize()]
        public void SqrTest() {
            var d = 9;
            mis = new Mis();
            Assert.AreEqual(81d, Mis.Sqr(d));
        }

        [TestMethod()]
        public void Synch_0Test() {
            mis.Synch_0(1);           
        }

        [TestMethod()]
        public void Synch_1Test() {
            mis.Synch_0(1);
            mis.Synch_1();
        }

        [TestMethod()]
        public void Synch_2Test() {
            mis.Synch_0(1);
            mis.Synch_1();
            mis.Synch_2();
        }

        [TestMethod()]
        public void Delta_Test() {
            mis.delta_i_rad[0] = 15;
            mis.delta_i_rad[1] = -15;
            mis.delta_i_rad[2] = 5;
            mis.delta_i_rad[3] = -10;
            var answ = mis.matr_lambda * mis.delta_i_rad;
        }

        [TestMethod()]
        public void Vector_Test() {
            mis.Vel.X = 200;
            mis.Vel.Y = 30;
            mis.Vel.Z = -20;
            var v0 = mis.Rebuild(10);
            var a0 = mis.f(10, v0);
            var s = mis.GetDiffPrms().Select(dp => dp.FullName).ToArray();
            foreach (var d in v0.ToArray().Concat(a0.ToArray()) ) {
                Assert.IsFalse(Double.IsNaN(d));
            }
        }
    }
}