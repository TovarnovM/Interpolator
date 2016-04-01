using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpolator.Tests
{
    [TestClass()]
    public class PotentGraff2DTests
    {
        [TestMethod()]
        public void GetVTest()
        {
            var testDummy = new PotentGraff2P();

            var testLine = new LevelLine(0);
            testLine.AddPoint( 2 , 0 );
            testLine.AddPoint(1.5, 1.5);
            testLine.AddPoint(0, 2);
            testLine.AddPoint(0, 5);
            testDummy.AddElement(testLine.Value, testLine);

            testLine = new LevelLine(1);
            testLine.AddPoint(2.5, 0);
            testLine.AddPoint(2, 2);
            testLine.AddPoint(1, 3);
            testLine.AddPoint(0.5, 5);
            testDummy.AddElement(testLine.Value, testLine);

            testLine = new LevelLine(2);
            testLine.AddPoint(3, 0);
            testLine.AddPoint(3, 2);
            testLine.AddPoint(2.5, 3);
            testLine.AddPoint(3.5, 4);
            testLine.AddPoint(6, 5);
            testDummy.AddElement(testLine.Value, testLine);

            testLine = new LevelLine(3);
            testLine.AddPoint(3.5, 0);
            testLine.AddPoint(4, 2);
            testLine.AddPoint(5, 2.5);
            testLine.AddPoint(6, 1.5);
            testDummy.AddElement(testLine.Value, testLine);

            testLine = new LevelLine(4);
            testLine.AddPoint(4, 0);
            testLine.AddPoint(5, 1);
            testLine.AddPoint(6, 0.5);
            testDummy.AddElement(testLine.Value, testLine);

            testLine = new LevelLine(5);
            testLine.AddPoint(4.5, 0);
            testLine.AddPoint(5, 0.5);
            testLine.AddPoint(6, 0);
            testDummy.AddElement(testLine.Value, testLine);

            Assert.AreEqual(6, testDummy.Count);

            Assert.AreEqual(0, testDummy.GetV(0, 0), 0.0001);
            Assert.AreEqual(0, testDummy.GetV(1, 1), 0.0001);
            Assert.AreEqual(0.5, testDummy.GetV(0.5, 3), 0.1);
            Assert.AreEqual(0.2, testDummy.GetV(1, 2), 0.2);
            Assert.AreEqual(1.8, testDummy.GetV(2.5, 4), 0.15);
            Assert.AreEqual(2.5, testDummy.GetV(5, 3.5), 0.15);
            Assert.AreEqual(2.9, testDummy.GetV(4.5, 2.5), 0.15);
            Assert.AreEqual(2, testDummy.GetV(2.5, 3), 0.001);
            Assert.AreEqual(5, testDummy.GetV(5, 0), 0.0001);

            Assert.AreEqual(true, testDummy.ValidData());

            testLine = new LevelLine(6);
            testLine.AddPoint(5, 0);
            testLine.AddPoint(6, 1);
            testDummy.AddElement(testLine.Value, testLine);

            Assert.AreEqual(false, testDummy.ValidData());

        }
    }
}