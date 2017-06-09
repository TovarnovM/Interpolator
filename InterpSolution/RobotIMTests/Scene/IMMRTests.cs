using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotIM.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotIM.Scene.Tests {
    [TestClass()]
    public class IMMRTests {
        [TestMethod()]
        public void GetDBToTest() {
            var rob = new IMMR("ss", null);
            rob.noiseDB = 100;
            Assert.AreEqual(100, rob.GetDBTo(1), 0.001);
            Assert.AreEqual(100 - 3, rob.GetDBTo(1.5), 0.001);
            Assert.AreEqual(100-6, rob.GetDBTo(2), 0.001);
            Assert.AreEqual(100 - 9, rob.GetDBTo(3), 0.001);
            
        }
    }
}