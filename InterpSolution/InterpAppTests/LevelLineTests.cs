using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Interpolator.Tests
{
    [TestClass()]
    public class LevelLineTests
    {
        private LevelLine testDummy;
        [TestMethod(), TestInitialize()]
        public void AddPointTest()
        {
            testDummy = new LevelLine(0.7);
            Assert.AreEqual(0.7, testDummy.Value, 0.001);
            testDummy.AddPoints(new double[][]  {
                new double[] { -2,-3 },
                new double[] { -2,-1 },
                new double[] { -1, 1 },
                new double[] {  1, 2 },
                new double[] {  1, 3 },
                new double[] {  -1.5, 4 }
            });
            Assert.AreEqual(6, testDummy.Count);
        }

        [TestMethod()]
        public void NearestToPointTest()
        {
            Assert.AreEqual<Vector>(new Vector(-2, -3), testDummy.NearestToPoint(new Vector(-3, -4)));
            Assert.AreEqual((new Vector(-2, -3) - new Vector(-3, -4)).LengthSquared,
                            (testDummy.NearestToPoint(new Vector(-3, -4)) - new Vector(-3, -4)).LengthSquared);

            Assert.AreEqual<Vector>(new Vector(-2, -2), testDummy.NearestToPoint(new Vector(-1, -2)));
            Assert.AreEqual((new Vector(-2, -2) - new Vector(-1, -2)).LengthSquared,
                            (testDummy.NearestToPoint(new Vector(-1, -2)) - new Vector(-1, -2)).LengthSquared);

            Assert.AreEqual<Vector>(new Vector(-2, -1), testDummy.NearestToPoint(new Vector(-3, -0.5)));
            Assert.AreEqual((new Vector(-2, -1) - new Vector(-3, -0.5)).LengthSquared,
                            (testDummy.NearestToPoint(new Vector(-3, -0.5)) - new Vector(-3, -0.5)).LengthSquared);

            Assert.AreEqual<Vector>(new Vector(-1.5, 0), testDummy.NearestToPoint(new Vector(-3.5, 1)));
            Assert.AreEqual((new Vector(-1.5, 0) - new Vector(-3.5, 1)).LengthSquared,
                            (testDummy.NearestToPoint(new Vector(-3.5, 1)) - new Vector(-3.5, 1)).LengthSquared);

            Assert.AreEqual<Vector>(new Vector(1, 2.5), testDummy.NearestToPoint(new Vector(0.5, 2.5)));
            Assert.AreEqual((new Vector(1, 2.5) - new Vector(0.5, 2.5)).LengthSquared,
                            (testDummy.NearestToPoint(new Vector(0.5, 2.5)) - new Vector(0.5, 2.5)).LengthSquared);

            Assert.AreEqual<Vector>(new Vector(-1.5, 4), testDummy.NearestToPoint(new Vector(-2.5, 4)));
            Assert.AreEqual((new Vector(-1.5, 4) - new Vector(-2.5, 4)).LengthSquared,
                            (testDummy.NearestToPoint(new Vector(-2.5, 4)) - new Vector(-2.5, 4)).LengthSquared);
        }

        [TestMethod()]
        public void BoundingBoxesIntersectTest()
        {
            Vector v1 = new Vector(-2, 3); Vector v2 = new Vector(0, 3); Vector v3 = new Vector(2, 3); Vector v4 = new Vector(3, 3);
            Vector v5 = new Vector(-2, 2); Vector v6 = new Vector(0, 2); Vector v7 = new Vector(2, 2); Vector v8 = new Vector(3, 2);
            Vector v9 = new Vector(-2, 1); Vector v10 = new Vector(0, 1); Vector v11 = new Vector(2, 1); Vector v12 = new Vector(3, 1);
            Vector v13 = new Vector(-2, 0); Vector v14 = new Vector(0, 0); Vector v15 = new Vector(2, 0); Vector v16 = new Vector(3, 0);

            Assert.AreEqual(true, LevelLine.BoundingBoxesIntersect(v1, v11, v14, v8));
            Assert.AreEqual(true, LevelLine.BoundingBoxesIntersect(v5, v8, v14, v3));
            Assert.AreEqual(true, LevelLine.BoundingBoxesIntersect(v11, v1, v8, v14));
            Assert.AreEqual(true, LevelLine.BoundingBoxesIntersect(v8, v5, v3, v14));
            Assert.AreEqual(true, LevelLine.BoundingBoxesIntersect(v1, v16, v7, v10));
            Assert.AreEqual(true, LevelLine.BoundingBoxesIntersect(v3, v10, v7, v5));
            Assert.AreEqual(true, LevelLine.BoundingBoxesIntersect(v14, v8, v9, v3));
            Assert.AreEqual(true, LevelLine.BoundingBoxesIntersect(v3, v3, v3, v3));
            Assert.AreEqual(true, LevelLine.BoundingBoxesIntersect(v1, v6, v6, v11));
            Assert.AreEqual(true, LevelLine.BoundingBoxesIntersect(v4, v10, v9, v14));
            Assert.AreEqual(true, LevelLine.BoundingBoxesIntersect(v4, v10, v14, v9));

            Assert.AreEqual(false, LevelLine.BoundingBoxesIntersect(v1, v6, v16, v11));
            Assert.AreEqual(false, LevelLine.BoundingBoxesIntersect(v10, v13, v11, v16));
            Assert.AreEqual(false, LevelLine.BoundingBoxesIntersect(v1, v1, v8, v14));
            Assert.AreEqual(false, LevelLine.BoundingBoxesIntersect(v7, v4, v12, v13));

        }

        [TestMethod()]
        public void CrossProductTest()
        {
            var l = new List<Vector>(52);
            l.Add(new Vector(0, 0));
            l.Add(new Vector(1, 1));
            var rnd = new Random();
            for (int i = 0; i < 50; i++)
            {
                l.Add(new Vector(rnd.NextDouble(), rnd.NextDouble()));
            }
            foreach (var p1 in l)
            {
                foreach (var p2 in l)
                {
                    double diff = LevelLine.CrossProduct(p1, p2) + LevelLine.CrossProduct(p2, p1);
                    Assert.AreEqual(0, diff, LevelLine.EPSILON);

                }
            }

        }

        [TestMethod()]
        public void IsPointOnLineTest()
        {
            var a0 = new Vector(0, 0);
            var a1 = new Vector(4, 4);
            var p = new Vector(3, 3);
            Assert.AreEqual(false, LevelLine.IsPointRightOfLine(a0, a1, p));

            a0 = new Vector(0, 0);
            a1 = new Vector(4, 4);
            Assert.AreEqual(false, LevelLine.IsPointRightOfLine(a0, a1, p));
        }

        [TestMethod()]
        public void IsPointRightOfLineTest()
        {
            var a0 = new Vector(0, 0);
            var a1 = new Vector(0, 7);
            var p = new Vector(5, 5);
            Assert.AreEqual(true, LevelLine.IsPointRightOfLine(a0, a1, p));

            p = new Vector(-5, 5);
            Assert.AreEqual(false, LevelLine.IsPointRightOfLine(a0, a1, p));
        }

        /* Tests for doLinesIntersect https://martin-thoma.com/how-to-check-if-two-line-segments-intersect/*/
        [TestMethod()]
        public void testLinesDontIntesectF1()
        {
            var a0 = new Vector(0, 0);
            var a1 = new Vector(7, 7);
            var b0 = new Vector(3, 4);
            var b1 = new Vector(4, 5);
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDontIntesectF2()
        {
            var a0 = new Vector(-4, 4);
            var a1 = new Vector(-2, 1);
            var b0 = new Vector(-2, 3);
            var b1 = new Vector(0, 0);
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDontIntesectF3()
        { 
            var a0 = new Vector(0, 0);
            var a1 = new Vector(0, 1);
            var b0 = new Vector(2, 2);
            var b1 = new Vector(2, 3);
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDontIntesectF4()
        {
            var a0 = new Vector(0, 0);
            var a1 = new Vector(0, 1);
            var b0 = new Vector(2, 2);
            var b1 = new Vector(3, 2);
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDontIntesectF5()
        {
            var a0 = new Vector(-1, -1);
            var a1 = new Vector(2, 2);
            var b0 = new Vector(3, 3);
            var b1 = new Vector(5, 5);
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDontIntesectF6()
        {
            var a0 = new Vector(0, 0);
            var a1 = new Vector(1, 1);
            var b0 = new Vector(2, 0);
            var b1 = new Vector(0.5, 2);
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDontIntesectF7()
        {

            var a0 = new Vector(1, 1);
            var a1 = new Vector(4, 1);
            var b0 = new Vector(2, 2);
            var b1 = new Vector(3, 2);
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDontIntesectF8()
        {
            var a0 = new Vector(0, 5);
            var a1 = new Vector(6, 1);
            var b0 = new Vector(2, 1);
            var b1 = new Vector(2, 2);
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(false, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDoIntesectT1()
        {
            var a0 = new Vector(0, -2);
            var a1 = new Vector(0, 2);
            var b0 = new Vector(-2, 0);
            var b1 = new Vector(2, 0);
            Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDoIntesectT2()
        {
            var a0 = new Vector(5, 5);
            var a1 = new Vector(0, 0);
            var b0 = new Vector(1, 1);
            var b1 = new Vector(8, 2);
            Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDoIntesectT3()
        {
            var a0 = new Vector(-1, 0);
            var a1 = new Vector(0, 0);
            var b0 = new Vector(-1, -1);
            var b1 = new Vector(-1, 1);
            Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDoIntesectT4()
        {
            var a0 = new Vector(0, 2);
            var a1 = new Vector(2, 2);
            var b0 = new Vector(2, 0);
            var b1 = new Vector(2, 4);
            Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDoIntesectT5()
        {
            var a0 = new Vector(0, 0);
            var a1 = new Vector(5, 5);
            var b0 = new Vector(1, 1);
            var b1 = new Vector(3, 3);
            Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b0, b1));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b1, b0));
            Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
        }
        [TestMethod()]
        public void testLinesDoIntesectT6()
        {
            for (int i = 0; i < 50; i++)
            {
                var rnd = new Random();
                double ax = rnd.NextDouble();
                double ay = rnd.NextDouble();
                double bx = rnd.NextDouble();
                double by = rnd.NextDouble();

                var a0 = new Vector(ax, ay);
                var a1 = new Vector(bx, by);
                var b0 = new Vector(ax, ay);
                var b1 = new Vector(bx, by);
                Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b0, b1));
                Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
                Assert.AreEqual(true, LevelLine.LinesIntersect(a0, a1, b1, b0));
                Assert.AreEqual(true, LevelLine.LinesIntersect(a1, a0, b0, b1));
            }
        }
    }
}