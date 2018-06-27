using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Interpolator.Tests {
    [TestClass()]
    public class PolygonTests {
        [TestMethod()]
        public void IsInsideTest() {
            var testDummy = new Polygon(0.7);
            testDummy.AddPoints(new double[][]  {
                new double[] { -1,-1 },
                new double[] { 0,1 },
                new double[] { 2, 2 },
                new double[] {  3, -1.5 },
                new double[] {  -1, -1 }
            });
            Assert.IsFalse(testDummy.IsInside(new Vector(1, -3)));
        }
        [TestMethod()]
        public void IsInsideTest2() {
            var testDummy = new Polygon(0.7);
            testDummy.AddPoints(new double[][]  {
                new double[] { -1,-1 },
                new double[] { 0,1 },
                new double[] { 2, 2 },
                new double[] {  3, -1.5 },
                new double[] {  -1, -1 }
            });
            Assert.IsTrue(testDummy.IsInside(new Vector(1, 0)));
        }
        [TestMethod()]
        public void IsClosedTest() {
            var testDummy = new Polygon(0.7);
            testDummy.AddPoints(new double[][]  {
                new double[] { -1,-1 },
                new double[] { 0,1 },
                new double[] { 2, 2 },
                new double[] {  3, -1.5 },
                new double[] {  -1, -1 }
            });
            Assert.IsTrue(testDummy.IsClosed);
        }
        [TestMethod()]
        public void IsClosedTest2() {
            var testDummy = new Polygon(0.7);
            testDummy.AddPoints(new double[][]  {
                new double[] { -1,-1 },
                new double[] { 0,1 },
                new double[] { 2, 2 },
                new double[] {  3, -1.5 }
            });
            Assert.IsFalse(testDummy.IsClosed);
            testDummy.Close();
            Assert.IsTrue(testDummy.IsClosed);
        }

        [TestMethod()]
        public void GetSquareTest() {
            var testDummy = new Polygon(0.7);
            testDummy.AddPoints(new double[][]  {
                new double[] { 0,0 },
                new double[] { 0,1 },
                new double[] { 1, 1 },
                new double[] { 1, 0 }
            });
            Assert.AreEqual(1d, testDummy.GetSquare(), 0.0001);
            testDummy.Close();
            Assert.AreEqual(1d, testDummy.GetSquare(), 0.0001);
        }
    }
}