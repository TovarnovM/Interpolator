using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIntegrator.Tests {
    [TestClass()]
    public class PositionTests {
        [TestMethod()]
        public void FindParamTest() {
            var vec = new Position3D();
            var sc = vec.FindParam("z");
            Assert.AreEqual("Z",sc.Name);
            sc = vec.FindParam("X");
            Assert.AreEqual("X",sc.Name);
            sc = vec.FindParam("y");
            Assert.AreEqual("Y",sc.Name);
        }


    }
}