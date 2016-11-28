using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotSim;
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
            Assert.AreEqual(15+1,v0.Y,0.00001);
            Assert.AreEqual(0+2,v0.Z,0.00001);
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
            Assert.AreEqual(15+1,v0.Y,0.00001);
            Assert.AreEqual( 2,v0.Z,0.00001);
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
            Assert.AreEqual( 1,v0.Y,0.00001);
            Assert.AreEqual(15+2,v0.Z,0.00001);
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
            Assert.AreEqual( 2,v0.Z,0.00001);
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
    }
}