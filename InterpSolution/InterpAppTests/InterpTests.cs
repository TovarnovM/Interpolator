using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpolator.Tests {
    [TestClass()]
    public class InterpTests {
        [TestMethod()]
        public void RepeatShiftTest() {
            var interp = new InterpXY();
            interp.Add(1.5,0);
            interp.Add(2,0);
            interp.Add(3,0);

            double sh = 6.5;
            interp.RepeatShift(ref sh);
            Assert.AreEqual(2.0,sh,0.0001);

            sh = 2.0;
            interp.RepeatShift(ref sh);
            Assert.AreEqual(2.0,sh,0.0001);

            sh = -5.0;
            interp.RepeatShift(ref sh);
            Assert.AreEqual(2.5,sh,0.0001);

            sh = -3.0;
            interp.RepeatShift(ref sh);
            Assert.AreEqual(1.5,sh,0.0001);

            sh = 0.0;
            interp.RepeatShift(ref sh);
            Assert.AreEqual(1.5,sh,0.0001);

            interp.Data.Clear();
            interp.Add(-1.5,0);
            interp.Add(-2,0);
            interp.Add(-3,0);

            sh = -5.0;
            interp.RepeatShift(ref sh);
            Assert.AreEqual(-2.0,sh,0.0001);

            sh = -3.0;
            interp.RepeatShift(ref sh);
            Assert.AreEqual(-3.0,sh,0.0001);

            sh = -1.0;
            interp.RepeatShift(ref sh);
            Assert.AreEqual(-2.5,sh,0.0001);

            sh = 0.0;
            interp.RepeatShift(ref sh);
            Assert.AreEqual(-3.0,sh,0.0001);

            sh = 2.0;
            interp.RepeatShift(ref sh);
            Assert.AreEqual(-2.5,sh,0.0001);
        }

        [TestMethod()]
        public void RepeatShiftTest2() {
            var interp = new InterpXY();
            interp.Add(-1,0);
            interp.Add(1,2);
            interp.Add(2,2);
            interp.ET_right = ExtrapolType.etRepeat;
            interp.ET_left = ExtrapolType.etRepeat;

            Assert.AreEqual(1.0,interp[-3],0.0001);
            Assert.AreEqual(1.0,interp[3],0.0001);
            Assert.AreEqual(0.5,interp[5.5],0.0001);
            Assert.AreEqual(2.0,interp[4.5],0.0001);
            Assert.AreEqual(0,interp[-4.0],0.0001);
        }
    }
}