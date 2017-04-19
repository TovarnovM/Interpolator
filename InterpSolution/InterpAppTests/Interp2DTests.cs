using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpolator.Tests {
    [TestClass()]
    public class Interp2DTests {

        [TestMethod()]
        public void ImportDataFromMatrixTest() {
            var interp2D = new Interp2D();
            interp2D.ImportDataFromMatrix(new double[4,5]
                {   { 0, 0, 20, 30, 40},
                    { 0, -1,-2,-3,-3},
                    { 2, 1, 2, 3, 1},
                    { -1,0, 3, 0, -1}      });
            Assert.AreEqual(1.5,interp2D.GetV(-1,10),0.00001);
            Assert.AreEqual(0,interp2D.GetV(1,21),0.00001);
            Assert.AreEqual(2,interp2D.GetV(2,35),0.0001);


        }
        [TestMethod()]
        public void ImportDataFromMatrixTest2() {
            var interp2D = new Interp2D();
            var i1 = new InterpXY();
            i1.Add(1,1);
            i1.Add(2,2);
            interp2D.AddElement(77,i1);
            var i2 = new InterpXY();
            i2.Add(10,10);
            i2.Add(20,20);
            interp2D.AddElement(0,i2);
            var answ = interp2D.GetV(1.5,77);
            Assert.AreEqual(1.5,answ,0.00001);

        }
    }
}