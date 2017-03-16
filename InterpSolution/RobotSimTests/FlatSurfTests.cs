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
    public class FlatSurfTests {
        [TestMethod()]
        public void GetIntersectTest1() {
            var s = new FlatSurf(1,1,new Vector3D(100,1,22));
            var l = new Line3D(new Vector3D(3,3,3),new Vector3D(2,2,2));
            var c = s.GetIntersect(l);
            Assert.IsTrue(Vector3D.ApproxEqual(c,new Vector3D(1,1,1)));
        }
        [TestMethod()]
        public void GetIntersectTes2() {
            var s = new FlatSurf(1,1,new Vector3D(1,10,22),new Vector3D(1,0,0));
            var l = new Line3D(new Vector3D(3,3,3),new Vector3D(2,2,2));
            var c = s.GetIntersect(l);
            Assert.IsTrue(Vector3D.ApproxEqual(c,new Vector3D(1,1,1)));
        }
        [TestMethod()]
        public void GetIntersectTes3() {
            var s = new FlatSurf(1,1,new Vector3D(-21,10,1),new Vector3D(0,0,1));
            var l = new Line3D(new Vector3D(3,3,3),new Vector3D(2,2,2));
            var c = s.GetIntersect(l);
            Assert.IsTrue(Vector3D.ApproxEqual(c,new Vector3D(1,1,1)));
        }
        [TestMethod()]
        public void GetIntersectTest4() {
            var s = new FlatSurf(1,1,new Vector3D(1,1,1));
            var l = new Line3D(new Vector3D(3,3,3),new Vector3D(1,1,1));
            var c = s.GetIntersect(l);
            Assert.IsTrue(Vector3D.ApproxEqual(c,new Vector3D(1,1,1)));
        }
        [TestMethod()]
        public void GetIntersectTest5() {
            var s = new FlatSurf(1,1,new Vector3D(6,1,44));
            var l = new Line3D(new Vector3D(3,3,3),new Vector3D(1,1,1));
            var c = s.GetIntersect(l);
            Assert.IsTrue(Vector3D.ApproxEqual(c,new Vector3D(1,1,1)));
        }

        Vector3D GetRndVec(Random rnd) {
            return new Vector3D(rnd.NextDouble(),rnd.NextDouble(),rnd.NextDouble());
        }

        [TestMethod()]
        public void GetIntersectTest_1() {
            var rnd = new Random();
            for(int i = 0; i < 1000; i++) {
                var s1 = new FlatSurf(1,1,GetRndVec(rnd),GetRndVec(rnd));
                var s2 = new FlatSurf(1,1,GetRndVec(rnd),GetRndVec(rnd));
                var l = FlatSurf.GetIntersect(s1,s2);
                if(l == null)
                    Assert.Fail();
                Assert.IsTrue(s1.BelongLine(l));
                Assert.IsTrue(s2.BelongLine(l));
            }
        }
    }
}