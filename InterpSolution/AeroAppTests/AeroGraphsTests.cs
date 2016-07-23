using Microsoft.VisualStudio.TestTools.UnitTesting;
using RocketAero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interpolator;

namespace RocketAero.Tests
{
    [TestClass()]
    public class AeroGraphsTests
    {
        private AeroGraphs sample;
        [TestMethod(), TestInitialize()]
        public void AeroGraphsTest()
        {
            sample = new AeroGraphs();
        }

        [TestMethod()]
        public void GetClone() {
            AeroGraphs clone = (AeroGraphs)sample.Clone();

            Assert.AreEqual(0.019, clone.GetV("3_5_3D", 0, 1.6, 1.5), 0.05);
            Assert.AreEqual(0.029, clone.GetV("3_5_3D", 0, 1.1, 0.4), 0.05);
            Assert.AreEqual(0.014, clone.GetV("3_5_3D", 0, 1.7, 4), 0.05);
            Assert.AreEqual(0.028, clone.GetV("3_5_3D", 0, 0.3, 1.7), 0.05);
            Assert.AreEqual(0.021, clone.GetV("3_5_3D", 1, 0.9, 2.2), 0.05);
            Assert.AreEqual(0.029, clone.GetV("3_5_3D", 1, 0.2, 1.5), 0.05);
            Assert.AreEqual(0.024, clone.GetV("3_5_3D", 1, 1.5, -1.2), 0.05);
            Assert.AreEqual(0.021, clone.GetV("3_5_3D", 1, 1.7, 0.6), 0.05);
            Assert.AreEqual(0.014, clone.GetV("3_5_3D", 2, 1.6, 3.5), 0.05);
            Assert.AreEqual(0.025, clone.GetV("3_5_3D", 2, 0.7, 0.8), 0.05);
            Assert.AreEqual(0.022, clone.GetV("3_5_3D", 2, 1.4, -1), 0.05);
            Assert.AreEqual(0.017, clone.GetV("3_5_3D", 2, 1.6, -2.5), 0.05);
            Assert.AreEqual(0.018, clone.GetV("3_5_3D", 3, 1, -1.7), 0.05);
            Assert.AreEqual(0.011, clone.GetV("3_5_3D", 3, 1.7, 5.2), 0.05);
            Assert.AreEqual(0.017, clone.GetV("3_5_3D", 3, 0.9, 2.4), 0.05);
            Assert.AreEqual(0.026, clone.GetV("3_5_3D", 3, 0.2, 1), 0.05);

            Assert.AreEqual(0.83, clone.GetV("3_22", 3, 0.3), 0.1);

            Assert.AreEqual("4_5_5", clone.CutMyString("__4_5_5_4D"));
            Assert.AreEqual("4_5_5", clone.CutMyString("4_5_5_4D"));

            Assert.AreEqual(0.57, clone.GetV("3_17", 2.2, 0.8, 0.6, 0.5), 0.2); //требуемая точность - delta - ?
            Assert.AreEqual(0.15, clone.GetV("3_17", 0.2, 1.2, 0.2, 1), 0.01);
            Assert.AreEqual(2.5, clone.GetV("3_17", 1, 0, 0, 1), 0.01);
        }


        [TestMethod()]
        public void GetParamsTest()
        {
            Assert.AreEqual(0.83, sample.GetV("3_22", 3, 0.3), 0.1);
        }


        [TestMethod()]
        public void CutMyStringTest()
        {
            Assert.AreEqual("4_5_5", sample.CutMyString("__4_5_5_4D"));
            Assert.AreEqual("4_5_5", sample.CutMyString("4_5_5_4D"));
            Assert.AreEqual("4_5_5", sample.CutMyString("4_5_5"));
        }


        [TestMethod()]
        public void GetVTest_3_17()
        {
            Assert.AreEqual(0.57, sample.GetV("3_17", 2.2, 0.8, 0.6, 0.5), 0.2); //требуемая точность - delta - ?
            Assert.AreEqual(0.15, sample.GetV("3_17", 0.2, 1.2, 0.2, 1), 0.01);
            Assert.AreEqual(2.5, sample.GetV("3_17", 1, 0, 0, 1), 0.01);
            Assert.AreEqual(0.5, sample.GetV("3_17", 2, 1.6, 0.4, 0), 0.01);
        }

        [TestMethod()]
        public void GetVTest_3_2()
        {
            Assert.AreEqual(0.035, sample.GetV("3_2", 0.2, -0.4), 0.05);
            Assert.AreEqual(0.047, sample.GetV("3_2", 0.8, 1), 0.05);
            Assert.AreEqual(0.06, sample.GetV("3_2", 4, 2.4), 0.05);
        }

        [TestMethod()]
        public void GetVTest_3_3()
        {
            Assert.AreEqual(0.035, sample.GetV("3_3", 4, -0.4), 0.05);
            Assert.AreEqual(0.045, sample.GetV("3_3", 0.4, 0.8), 0.05);
            Assert.AreEqual(0.045, sample.GetV("3_3", 1.5, 2), 0.05);
        }

        [TestMethod()]
        public void GetVTest_3_4()
        {
            Assert.AreEqual(0.028, sample.GetV("3_4", 1, 0.68), 0.05);
            Assert.AreEqual(0.042, sample.GetV("3_4", 0, 0.3), 0.05);
        }

        [TestMethod()]
        public void GetVTest_3_5_3D()
        {
            Assert.AreEqual(0.019, sample.GetV("3_5_3D", 0, 1.6, 1.5), 0.05);
            Assert.AreEqual(0.029, sample.GetV("3_5_3D", 0, 1.1, 0.4), 0.05);
            Assert.AreEqual(0.014, sample.GetV("3_5_3D", 0, 1.7, 4), 0.05);
            Assert.AreEqual(0.028, sample.GetV("3_5_3D", 0, 0.3, 1.7), 0.05);
            Assert.AreEqual(0.021, sample.GetV("3_5_3D", 1, 0.9, 2.2), 0.05);
            Assert.AreEqual(0.029, sample.GetV("3_5_3D", 1, 0.2, 1.5), 0.05);
            Assert.AreEqual(0.024, sample.GetV("3_5_3D", 1, 1.5, -1.2), 0.05);
            Assert.AreEqual(0.021, sample.GetV("3_5_3D", 1, 1.7, 0.6), 0.05);
            Assert.AreEqual(0.014, sample.GetV("3_5_3D", 2, 1.6, 3.5), 0.05);
            Assert.AreEqual(0.025, sample.GetV("3_5_3D", 2, 0.7, 0.8), 0.05);
            Assert.AreEqual(0.022, sample.GetV("3_5_3D", 2, 1.4, -1), 0.05);
            Assert.AreEqual(0.017, sample.GetV("3_5_3D", 2, 1.6, -2.5), 0.05);
            Assert.AreEqual(0.018, sample.GetV("3_5_3D", 3, 1, -1.7), 0.05);
            Assert.AreEqual(0.011, sample.GetV("3_5_3D", 3, 1.7, 5.2), 0.05);
            Assert.AreEqual(0.017, sample.GetV("3_5_3D", 3, 0.9, 2.4), 0.05);
            Assert.AreEqual(0.026, sample.GetV("3_5_3D", 3, 0.2, 1), 0.05);
        }

        [TestMethod()]
        public void GetVTest_3_16_3D()
        {
            Assert.AreEqual(0.96, sample.GetV("3_16_3D", 1, 4, 1), 0.1);
            Assert.AreEqual(0.77, sample.GetV("3_16_3D", 1, 0, 4.5), 0.1);
            Assert.AreEqual(0.82, sample.GetV("3_16_3D", 2, 0.6, 3.2), 0.1);
            Assert.AreEqual(0.84, sample.GetV("3_16_3D", 2, 2, 0.8), 0.1);
            Assert.AreEqual(0.75, sample.GetV("3_16_3D", 100, 0, 4.6), 0.1);
            Assert.AreEqual(0.79, sample.GetV("3_16_3D", 100, 2, 2.5), 0.1);
        }

        [TestMethod()]
        public void GetVTest_3_21()
        {
            Assert.AreEqual(0.97, sample.GetV("3_21", 1.5, 0.6), 0.1);
            Assert.AreEqual(0.93, sample.GetV("3_21", 2.5, 0.8), 0.1);
            Assert.AreEqual(0.95, sample.GetV("3_21", 3.5, 1.4), 0.1);
            Assert.AreEqual(1, sample.GetV("3_21", 4.5, 0.9), 0.1);
            Assert.AreEqual(0.8, sample.GetV("3_21", 1, 4.5), 0.1);
            Assert.AreEqual(0.86, sample.GetV("3_21", 0.8, 3), 0.1);
            Assert.AreEqual(1, sample.GetV("3_21", 0, 0), 0.1);
        }

        [TestMethod()]
        public void GetVTest_3_22()
        {
            Assert.AreEqual(0.83, sample.GetV("3_22", 3.5, 0.3), 0.1);
            Assert.AreEqual(0.82, sample.GetV("3_22", 1.5, 0.1), 0.1);
            Assert.AreEqual(0.91, sample.GetV("3_22", 4.5, 1), 0.1);
            Assert.AreEqual(0.96, sample.GetV("3_22", 1, 0.5), 0.1);
        }

        [TestMethod()]
        public void GetVTest_4_2()
        {
            Assert.AreEqual(0.003, sample.GetV("4_2", 0.7, 8000000), 0.01);
            Assert.AreEqual(0.0025, sample.GetV("4_2", 0.9, 2500000), 0.01);
            Assert.AreEqual(0.0037, sample.GetV("4_2", 0, 250000000), 0.01);
            Assert.AreEqual(0.0032, sample.GetV("4_2", 0.6, 9000000), 0.01);
            Assert.AreEqual(0.0054, sample.GetV("4_2", 0.3, 4500000), 0.01);
            Assert.AreEqual(0.002, sample.GetV("4_2", 0.5, 400000000), 0.01);
        }

        [TestMethod()]
        public void GetVTest_4_3()
        {
            Assert.AreEqual(0.81, sample.GetV("4_3", 3.5, 0.9), 0.05);
            Assert.AreEqual(0.92, sample.GetV("4_3", 1, 0.7), 0.05);
            Assert.AreEqual(0.39, sample.GetV("4_3", 5.5, 0), 0.05);
            Assert.AreEqual(0.98, sample.GetV("4_3", 0.8, 0.4), 0.05);
        }

        [TestMethod()]
        public void GetVTest_4_11()
        {
            Assert.AreEqual(0.28, sample.GetV("4_11", 3.5, 1.3), 0.05);
            Assert.AreEqual(0.255, sample.GetV("4_11", 1.6, 1.8), 0.05);
            Assert.AreEqual(0.13, sample.GetV("4_11", 2, 2.5), 0.05);
            Assert.AreEqual(0.04, sample.GetV("4_11", 4.1, 5), 0.05);
        }

        [TestMethod()]
        public void GetVTest_4_12()
        {
            Assert.AreEqual(0.24, sample.GetV("4_12", 1.6, 1.8), 0.05);
            Assert.AreEqual(0.125, sample.GetV("4_12", 2, 2.7), 0.05);
            Assert.AreEqual(0.062, sample.GetV("4_12", 3.6, 3.5), 0.05);
            Assert.AreEqual(0, sample.GetV("4_12", 0.8, 2.5), 0.05);
        }

        [TestMethod()]
        public void GetVTest_4_13()
        {
            Assert.AreEqual(1.5, sample.GetV("4_13", 2, 0), 0.1); //что-то не так с этим графиком - тесты не проходит
            Assert.AreEqual(1.1, sample.GetV("4_13", 3.4, 0.25), 0.1);
            Assert.AreEqual(0.7, sample.GetV("4_13", 1.6, 0.5), 0.1);
            Assert.AreEqual(0.1, sample.GetV("4_13", 1, 1), 0.1);
            Assert.AreEqual(0, sample.GetV("4_13", 0.6, 2), 0.1);
        }
    }
}
