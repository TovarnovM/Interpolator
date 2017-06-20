using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpolator.Tests {
    [TestClass()]
    public class InterpXY_finderTests {
        [TestMethod()]
        public void Get_IntegralTest() {
            var xy = new InterpXY();
            for (int i = 0; i < 11; i++) {
                xy.Add(i, i);
            }
            var res = xy.Get_Integral();
            Assert.AreEqual(50, res);
            
            for (int i = 0; i < 11 ; i++) {
                xy.Add(11+i, -10+i);
            }
            res = xy.Get_Integral();
            Assert.AreEqual(0, res);
        }
    }
}