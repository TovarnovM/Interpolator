using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sharp3D.Math.Core;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIntegrator.Tests {
    [TestClass()]
    public class MaterialObjectNewtonTests {
        [TestMethod()]
        public void GetVelWorldTest() {
            var o = new MaterialObjectNewton();
            var vc = new Vector3D(1,2,3);
            o.Vel.Vec3D = vc;
            var v = o.GetVelWorld(new Vector3D(10,10,10));
            Assert.IsTrue(Vector3D.ApproxEqual(vc,v,0.000001));
        }
        [TestMethod()]
        public void GetVelWorldTest1() {
            var o = new MaterialObjectNewton();
            var vc = new Vector3D(1,2,3);
            o.Vel.Vec3D = vc;
            o.Omega.X = 1;

            var v = o.GetVelWorld(new Vector3D(10,0,-10));
            Assert.IsTrue(Vector3D.ApproxEqual(vc + new Vector3D(0,10,0),v,0.000001));
        }
        [TestMethod()]
        public void GetVelWorldTest2() {
            var o = new MaterialObjectNewton();
            var vc = new Vector3D(1,2,3);
            o.Vel.Vec3D = vc;
            o.Omega.Y = 1;

            var v = o.GetVelWorld(new Vector3D(10,0,0));
            Assert.IsTrue(Vector3D.ApproxEqual(vc + new Vector3D(0,0,-10),v,0.000001));
        }
    }
}