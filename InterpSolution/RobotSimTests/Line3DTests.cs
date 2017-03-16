using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotSim;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotSim.Tests {
    [TestClass()]
    public class Line3DTests {
        [TestMethod()]
        public void EqualTest1() {
            var l1 = new Line3D(new Vector3D(1,2,3),new Vector3D(2,3,4));
            var l2 = new Line3D(new Vector3D(4,5,6),new Vector3D(2,3,4));
            Assert.IsTrue(l1.Equal(l2));
            Assert.IsTrue(l2.Equal(l1));
        }
        [TestMethod()]
        public void EqualTest2() {
            var l1 = new Line3D(new Vector3D(1,2,3),new Vector3D(2,3,4));
            var l2 = new Line3D(new Vector3D(4,5,1),new Vector3D(2,3,4));
            Assert.IsFalse(l1.Equal(l2));
            Assert.IsFalse(l2.Equal(l1));
        }
        [TestMethod()]
        public void EqualTest3() {
            var l1 = new Line3D(new Vector3D(1,2,3),new Vector3D(2,3,4));
            var l2 = new Line3D(new Vector3D(1,2,3),new Vector3D(2,3,4.01));
            Assert.IsFalse(l1.Equal(l2));
            Assert.IsFalse(l2.Equal(l1));
        }
        [TestMethod()]
        public void EqualTest4() {
            var l1 = new Line3D(new Vector3D(1,2,3),new Vector3D(2,3,4));
            var l2 = new Line3D(new Vector3D(4,5,6),new Vector3D(11,12,13));
            Assert.IsTrue(l1.Equal(l2));
            Assert.IsTrue(l2.Equal(l1));
        }
    }
}