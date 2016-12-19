using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiGenetic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;

namespace MultiGenetic.Tests {
    [TestClass()]
    public class GeneDoubleRangeTests {
        [TestMethod()]
        public void GetRandValue_NormTest1() {
            var dr = new GeneDoubleRange("test",3,7);
            Assert.AreEqual(7,dr.GetRandValue_Norm(15,1));
        }
        [TestMethod()]
        public void GetRandValue_NormTest2() {
            var dr = new GeneDoubleRange("test",3,7);
            Assert.AreEqual(3,dr.GetRandValue_Norm(0,0.5));
        }
        [TestMethod()]
        public void GetRandValue_NormTest3() {
            var dr = new GeneDoubleRange("test",3,7);
            Assert.AreEqual(5,dr.GetRandValue_Norm(5,0.0000000000000001));
        }
        [TestMethod()]
        public void GetRandValue_NormTest4() {
            var dr = new GeneDoubleRange("test",3,7);
            for(int i = 0; i < 1000; i++) {
                var r = dr.GetRandValue_Norm(8,1);
                Assert.IsTrue(8 - 3.5 <= r && r<=7);
            }
            
        }
        [TestMethod()]
        public void GetRandValue_NormTest5() {
            var dr = new GeneDoubleRange("test",3,7);
            for(int i = 0; i < 1000; i++) {
                var r = dr.GetRandValue_Norm(0,1);
                Assert.IsTrue(3 <= r && r <= 3.5);
            }

        }
    }
}