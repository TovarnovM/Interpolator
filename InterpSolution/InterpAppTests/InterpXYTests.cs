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
    public class InterpXYTests
    {
        private InterpXY sInterp1elem;
        private InterpXY sInterp2elem;
        private InterpXY sInterp5elem;

        [TestMethod(), TestInitialize()]
        public void AddElementTest()
        {
            sInterp1elem = new InterpXY();
            sInterp2elem = new InterpXY();
            sInterp5elem = new InterpXY();

            sInterp1elem.Add(3, 3);
            Assert.AreEqual(1, sInterp1elem.Count());

            sInterp2elem.Add(1, 1);
            sInterp2elem.Add(4, 4);
            Assert.AreEqual(2, sInterp2elem.Count());

            sInterp5elem.Add(-1, -2);
            sInterp5elem.Add(0, 0);
            sInterp5elem.Add(1, -1);
            sInterp5elem.Add(5, 1);
            sInterp5elem.Add(10, 7);
            Assert.AreEqual(5, sInterp5elem.Count());
        }
        [TestMethod()]
        public void InerpolMethodLineTest()
        {
            sInterp1elem.InterpType = InterpolType.itLine;
            Assert.AreEqual(3.0, sInterp1elem.GetV(3));

            sInterp2elem.InterpType = InterpolType.itLine;
            Assert.AreEqual(1.0, sInterp2elem.GetV(1));
            Assert.AreEqual(3.0, sInterp2elem.GetV(3));
            Assert.AreEqual(4.0, sInterp2elem.GetV(4));

            sInterp5elem.InterpType = InterpolType.itLine;
            Assert.AreEqual(-2.0, sInterp5elem.GetV(-1));
            Assert.AreEqual(-0.2, sInterp5elem.GetV(-0.1),0.0001);
            Assert.AreEqual(2.2, sInterp5elem.GetV(6), 0.0001);
            Assert.AreEqual(0.0, sInterp5elem.GetV(3));
            Assert.AreEqual(-0.5, sInterp5elem.GetV(2), 0.0001);
            Assert.AreEqual(7.0, sInterp5elem.GetV(10));
        }
        [TestMethod()]
        public void InerpolMethodStepTest()
        {
            sInterp1elem.InterpType = InterpolType.itStep;
            Assert.AreEqual(3.0, sInterp1elem.GetV(3));

            sInterp2elem.InterpType = InterpolType.itStep;
            Assert.AreEqual(1.0, sInterp2elem.GetV(1));
            Assert.AreEqual(1, sInterp2elem.GetV(3));
            Assert.AreEqual(4, sInterp2elem.GetV(4));

            sInterp5elem.InterpType = InterpolType.itStep;
            Assert.AreEqual(-2.0, sInterp5elem.GetV(-1));
            Assert.AreEqual(-2.0, sInterp5elem.GetV(-0.1));
            Assert.AreEqual(1.0, sInterp5elem.GetV(6));
            Assert.AreEqual(-1.0, sInterp5elem.GetV(2));
            Assert.AreEqual(7.0, sInterp5elem.GetV(10));
        }
        [TestMethod()]
        public void RemoveElementTest()
        {
            var tmpInterp = new InterpXY();
            tmpInterp.Add(1, 1);
            tmpInterp.Add(2, 1);
            tmpInterp.Add(3, 1);
            tmpInterp.RemoveElement(0);
            Assert.AreEqual(2, tmpInterp.Count());
            Assert.AreEqual(2, tmpInterp.Count());
            Assert.AreEqual(2, tmpInterp.Count());
        }
        [TestMethod()]
        public void SetNTest()
        {
            Assert.AreEqual(-1, sInterp1elem.SetN(-2));
            Assert.AreEqual(0, sInterp1elem.SetN(3));
            Assert.AreEqual(0, sInterp1elem.SetN(4));

            Assert.AreEqual(0, sInterp2elem.SetN(2));
            Assert.AreEqual(0, sInterp2elem.SetN(1));
            Assert.AreEqual(-1, sInterp2elem.SetN(-2));
            Assert.AreEqual(1, sInterp2elem.SetN(4));
            Assert.AreEqual(1, sInterp2elem.SetN(7));

            Assert.AreEqual(-1, sInterp5elem.SetN(-2));
            Assert.AreEqual(4, sInterp5elem.SetN(11));
            Assert.AreEqual(2, sInterp5elem.SetN(3));
            Assert.AreEqual(4, sInterp5elem.SetN(10));
            Assert.AreEqual(3, sInterp5elem.SetN(7));
            Assert.AreEqual(-1, sInterp5elem.SetN(-2));
            Assert.AreEqual(0, sInterp5elem.SetN(-1));
            Assert.AreEqual(0, sInterp5elem.SetN(-0.5));
            Assert.AreEqual(1, sInterp5elem.SetN(0));
        }
        [TestMethod()]
        public void ExtrapolForListWithOneElemTest()
        {
            sInterp1elem.ET_left = ExtrapolType.etZero;
            sInterp1elem.ET_right = ExtrapolType.etZero;
            Assert.AreEqual(0.0, sInterp1elem.GetV(-2));
            Assert.AreEqual(3.0, sInterp1elem.GetV(3));
            Assert.AreEqual(0.0, sInterp1elem.GetV(4));

            sInterp1elem.ET_left = ExtrapolType.etValue;
            sInterp1elem.ET_right = ExtrapolType.etValue;
            Assert.AreEqual(3.0, sInterp1elem.GetV(-2));
            Assert.AreEqual(3.0, sInterp1elem.GetV(3));
            Assert.AreEqual(3.0, sInterp1elem.GetV(4));
        }
        [TestMethod()]
        public void ExtrapolForListWithTwoElemTest()
        {
            sInterp2elem.ET_left = ExtrapolType.etZero;
            sInterp2elem.ET_right = ExtrapolType.etZero;
            Assert.AreEqual(0.0, sInterp2elem.GetV(0));
            Assert.AreEqual(1.0, sInterp2elem.GetV(1));
            Assert.AreEqual(0.0, sInterp2elem.GetV(5));
            Assert.AreEqual(4.0, sInterp2elem.GetV(4));
            if (sInterp2elem.GetV(2) < 2)
                Assert.Fail();
            sInterp2elem.ET_left = ExtrapolType.etValue;
            sInterp2elem.ET_right = ExtrapolType.etValue;
            Assert.AreEqual(1.0, sInterp2elem.GetV(0));
            Assert.AreEqual(1.0, sInterp2elem.GetV(1));
            Assert.AreEqual(4.0, sInterp2elem.GetV(5));
            Assert.AreEqual(4.0, sInterp2elem.GetV(4));
            if (sInterp2elem.GetV(2) < 2)
                Assert.Fail();
        }
        [TestMethod()]
        public void ExtrapolForListWithFiveElemTest()
        {
            sInterp5elem.ET_left = ExtrapolType.etZero;
            sInterp5elem.ET_right = ExtrapolType.etZero;
            Assert.AreEqual(0.0, sInterp5elem.GetV(-3));
            Assert.AreEqual(-2.0, sInterp5elem.GetV(-1));
            Assert.AreEqual(0.0, sInterp5elem.GetV(-3));
            Assert.AreEqual(-2.0, sInterp5elem.GetV(-1));
            Assert.AreEqual(0.0, sInterp5elem.GetV(13));
            Assert.AreEqual(7.0, sInterp5elem.GetV(10));
            if (sInterp5elem.GetV(5) < 1)
                Assert.Fail();
            sInterp5elem.ET_left = ExtrapolType.etValue;
            sInterp5elem.ET_right = ExtrapolType.etValue;
            Assert.AreEqual(-2.0, sInterp5elem.GetV(-3));
            Assert.AreEqual(-2.0, sInterp5elem.GetV(-1));
            Assert.AreEqual(7.0, sInterp5elem.GetV(13));
            Assert.AreEqual(-2.0, sInterp5elem.GetV(-3));
            Assert.AreEqual(-2.0, sInterp5elem.GetV(-1));
            Assert.AreEqual(7.0, sInterp5elem.GetV(10));
            if (sInterp5elem.GetV(5) < 1)
                Assert.Fail();
        }
    }
}