using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneDemSPH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDemSPH.Tests {
    [TestClass()]
    public class OneDemExampleTests {
        [TestMethod()]
        public void OneDemExampleTest() {
            var s = new OneDemExample();
            double RoTst = s.AllParticles.Sum(p =>
                p.M * KernelF.W(s.AllParticles[50].X - p.X,s.h));
            Assert.AreEqual(1.0,RoTst,0.00001);
        }
    }
}