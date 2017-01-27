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
    public class RbTrackTests {
        [TestMethod()]
        public void GetConnPVelTest1() {
            var tst = RbTrack.GetFlat(10,10);
            tst.Rebuild();
            tst.Omega.X = 3;

            tst.Vel.Vec3D = new Sharp3D.Math.Core.Vector3D(0,1,2);

            var v0 = tst.GetConnPVelWorld(0);

            Assert.AreEqual(0d,v0.X,0.00001);
            Assert.AreEqual(15 + 1,v0.Y,0.00001);
            Assert.AreEqual(0 + 2,v0.Z,0.00001);
        }

        [TestMethod()]
        public void GetConnPVelTest2() {
            var tst = RbTrack.GetFlat(10,10);
            tst.Rebuild();
            tst.Omega.Y = 3;

            tst.Vel.Vec3D = new Sharp3D.Math.Core.Vector3D(0,1,2);

            var v0 = tst.GetConnPVelWorld(0);

            Assert.AreEqual(-15,v0.X,0.00001);
            Assert.AreEqual(1,v0.Y,0.00001);
            Assert.AreEqual(-15 + 2,v0.Z,0.00001);
        }

        [TestMethod()]
        public void GetConnPVelTest3() {
            var tst = RbTrack.GetFlat(10,10);
            tst.Rebuild();
            tst.Omega.Z = 3;

            tst.Vel.Vec3D = new Sharp3D.Math.Core.Vector3D(0,1,2);

            var v0 = tst.GetConnPVelWorld(0);

            Assert.AreEqual(0,v0.X,0.00001);
            Assert.AreEqual(15 + 1,v0.Y,0.00001);
            Assert.AreEqual(2,v0.Z,0.00001);
        }

        [TestMethod()]
        public void GetConnPVelTest4() {
            var tst = RbTrack.GetFlat(10,10);
            tst.Rebuild();
            tst.Omega.X = 3;

            tst.Vel.Vec3D = new Sharp3D.Math.Core.Vector3D(0,1,2);

            var v0 = tst.GetConnPVelWorld(1);

            Assert.AreEqual(0,v0.X,0.00001);
            Assert.AreEqual(-15 + 1,v0.Y,0.00001);
            Assert.AreEqual(2,v0.Z,0.00001);
        }

        [TestMethod()]
        public void GetConnPVelTest5() {
            var tst = RbTrack.GetFlat(10,10);
            tst.Rebuild();
            tst.Omega.Y = -3;

            tst.Vel.Vec3D = new Sharp3D.Math.Core.Vector3D(0,1,2);

            var v0 = tst.GetConnPVelWorld(1);

            Assert.AreEqual(-15,v0.X,0.00001);
            Assert.AreEqual(1,v0.Y,0.00001);
            Assert.AreEqual(15 + 2,v0.Z,0.00001);
        }

        [TestMethod()]
        public void GetConnPVelTest6() {
            var tst = RbTrack.GetFlat(10,10);
            tst.Rebuild();
            tst.Omega.X = -30;
            tst.Omega.Z = -30;

            tst.Vel.Vec3D = new Sharp3D.Math.Core.Vector3D(0,1,2);

            var v0 = tst.GetConnPVelWorld(2);

            Assert.AreEqual(0,v0.X,0.00001);
            Assert.AreEqual(1,v0.Y,0.00001);
            Assert.AreEqual(2,v0.Z,0.00001);
        }

        [TestMethod()]
        public void GetConnPVelTest7() {
            var tst = RbTrack.GetFlat(10,10);
            tst.Rebuild();
            tst.Omega.X = -30;
            tst.Omega.Z = 30;

            tst.Vel.Vec3D = new Sharp3D.Math.Core.Vector3D(0,1,2);

            var v0 = tst.GetConnPVelWorld(3);

            Assert.AreEqual(0,v0.X,0.00001);
            Assert.AreEqual(1,v0.Y,0.00001);
            Assert.AreEqual(2,v0.Z,0.00001);
        }

        [TestMethod()]
        public void SetPositionTest1() {
            var tst = RbTrack.GetFlat();
            var point = new Vector3D(10,20,-30);
            int ind = 0;
            tst.SetPosition(ind,point);
            var posReal = tst.GetConnPWorld(ind);
            Assert.IsTrue(Vector3D.ApproxEqual(point,posReal,0.00001));
        }

        [TestMethod()]
        public void SetPositionTest2() {
            var tst = RbTrack.GetFlat();
            tst.Rebuild();
            var point = new Vector3D(10,20,-30);
            int ind = 0;
            tst.SetPosition(ind,point);
            

            var point2 = new Vector3D(-100,-33,0);
            int ind2 = 3;
            tst.SetPosition(ind2,point2,ind);
            var pos2Real = tst.GetConnPWorld(ind2);

            var r3 = (pos2Real - tst.Vec3D).Norm;
            var rp = (point2 - point).Norm;
            Assert.AreEqual(1d,r3 * rp,0.000001);

            var posReal = tst.GetConnPWorld(ind);
            Assert.IsTrue(Vector3D.ApproxEqual(point,posReal,0.00001));
        }

        [TestMethod()]
        public void SetPositionTest3() {
            var tst = RbTrack.GetFlat();
            tst.Rebuild();
            var point1 = new Vector3D(0,0,0);
            int ind1 = 0;
            tst.SetPosition(ind1,point1);

            var point2 = new Vector3D(1,1,0);
            int ind2 = 2;
            tst.SetPosition(ind2,point2,ind1);

            var point3 = new Vector3D(0,10,0);
            int ind3 = 1;
            tst.SetPosition(ind3,point3,ind1,ind2);

            var p3 = tst.GetConnPWorld(ind3);
            Assert.IsTrue(Vector3D.ApproxEqual((new Vector3D(-1,1,0)).Norm, p3.Norm,0.00001));


        }
        [TestMethod()]
        public void QuaternionTest() {
            var v1 = new Vector3D(1,1,1);
            var v2 = new Vector3D(-2,-2,-2);
            var q = QuaternionD.FromTwoVectors(v1,v2);

            var v3 = QuaternionD.Multiply(q,v1);
        }

        [TestMethod()]
        public void World_1Test() {
            var tst = RbTrack.GetFlat();
            tst.Rebuild();
            var point = new Vector3D(10,20,-30);
            int ind = 0;
            tst.SetPosition(ind,point);

            var p1 = tst.GetConnPWorld(1);
            Assert.IsTrue(Vector3D.ApproxEqual(tst.WorldTransform_1 * p1,tst.ConnP[1],0.00001));
        }

        [TestMethod()]
        public void World_2Test() {
            var tst = RbTrack.GetFlat();
            tst.Rebuild();
            var point = new Vector3D(10,20,-30);
            int ind = 0;
            tst.SetPosition(ind,point);

            var p1 = tst.WorldTransform * tst.WorldTransform_1;
            Assert.IsTrue(Vector3D.ApproxEqual( p1 * tst.ConnP[0],tst.ConnP[0],0.00001));
        }

    }
}