using Microsoft.VisualStudio.TestTools.UnitTesting;
using Experiment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp3D.Math.Core;

namespace Experiment.Tests {
    [TestClass()]
    public class ForceTests {
        [TestMethod()]
        public void GetMomentTest() {
            var f = new Force(new Vector3D(0,1,0));
            f.FPoint = new Position3D(new Vector3D(1,0,0));

            var answ = f.GetMoment();
            Assert.AreEqual(0d,(new Vector3D(0,0,1) - answ).GetLength(),0.000001);
        }

    }
}