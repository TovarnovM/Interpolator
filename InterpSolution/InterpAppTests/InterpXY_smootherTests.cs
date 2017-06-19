using Microsoft.VisualStudio.TestTools.UnitTesting;
using Interpolator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpolator.Tests {
    [TestClass()]
    public class InterpXY_smootherTests {
        [TestMethod()]
        public void GetSmootherByN_lineTest() {
            var xy = new InterpXY();
            for (int i = 0; i < 10; i++) {
                xy.Add(i, 10);
            }
            var xy_smuth = xy.GetSmootherByN_line(5);
            foreach (var it in xy_smuth.Data.Values) {
                Assert.AreEqual(10d, it.Value);
            }
        }
        [TestMethod()]
        public void GetSmootherByN_lineTes2t() {
            var xy = new InterpXY();
            for (int i = 0; i < 5; i++) {
                xy.Add(i, 10);
            }
            xy.Add(5, 20);
            for (int i = 6; i < 10; i++) {
                xy.Add(i, 10);
            }
            var xy_smuth = xy.GetSmootherByN_line(5);

        }
        [TestMethod()]
        public void GetSmootherByN_MedianTest() {
            var xy = new InterpXY();
            for (int i = 0; i < 5; i++) {
                xy.Add(i, 10);
            }
            xy.Add(5, 20);
            for (int i = 6; i < 10; i++) {
                xy.Add(i, 10);
            }
            var xy_smuth = xy.GetSmootherByN_Median(5);

        }
        [TestMethod()]
        public void GetSmootherByN_MedianTest2() {
            var xy = new InterpXY();
            for (int i = 0; i < 10; i++) {
                xy.Add(i, i);
            }
            var xy_smuth = xy.GetSmootherByN_Median(3);

        }
        [TestMethod()]
        public void GetSmootherByT_Uniform() {
            var xy = new InterpXY();
            for (int i = 0; i < 5; i++) {
                xy.Add(i, i);
            }
            for (int i = 5; i < 10; i++) {
                xy.Add(i, 5);
            }
            var xy_smuth = xy.GetSmootherByT_uniform(0,2.5);

        }
    }
}