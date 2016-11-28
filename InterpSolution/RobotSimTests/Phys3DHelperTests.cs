using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotSim;
using SimpleIntegrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp3D.Math;
using Sharp3D.Math.Core;

namespace RobotSim.Tests {
    [TestClass()]
    public class Phys3DHelperTests {

        class VelPosDummy : Position3D, IVelPos3D {

            public IPosition3D Vel { get; set; } = new Position3D();
            public VelPosDummy(double x, double y, double z, double vx, double vy, double vz) {
                X = x;
                Y = y;
                Z = z;
                Vel.X = vx;
                Vel.Y = vy;
                Vel.Z = vz;
            }

        }

        [TestMethod()]
        public void GetKMuForceTest1() {
            var me = new VelPosDummy(1,2,3,0,0,0);
            var toThis = new VelPosDummy(3,4,5,0,0,0);

            var diag = Math.Sqrt(3);
            var f = Phys3DHelper.GetKMuForce(me,toThis,10,10,2*diag);
            Assert.IsTrue(Vector3D.ApproxEqual(f,Vector3D.Zero,0.00000001));

        }
        [TestMethod()]
        public void GetKMuForceTest2() {
            var me = new VelPosDummy(1,2,3,0,0,0);
            var toThis = new VelPosDummy(3,4,5,0,0,0);

            var diag = Math.Sqrt(3);

            var f = Phys3DHelper.GetKMuForce(me,toThis,10,10,3 * diag);
            Assert.AreEqual(diag * 10,f.GetLength(),0.00001);
            Assert.AreEqual(-10,f.X,0.00001);
            Assert.AreEqual(-10,f.Y,0.00001);
            Assert.AreEqual(-10,f.Z,0.00001);
        }

        [TestMethod()]
        public void GetKMuForceTest3() {
            var me = new VelPosDummy(1,2,3,0,0,0);
            var toThis = new VelPosDummy(3,4,5,0,0,0);

            var diag = Math.Sqrt(3);
            var f = Phys3DHelper.GetKMuForce(me,toThis,10,10,2 * diag);
            Assert.IsTrue(Vector3D.ApproxEqual(f,Vector3D.Zero,0.00000001));

            me.Vel.Vec3D = new Vector3D(1,1,1);

            f = Phys3DHelper.GetKMuForce(me,toThis,10,10,2 * diag);
            Assert.AreEqual(diag * 10,f.GetLength(),0.00001);
            Assert.AreEqual(-10,f.X,0.00001);
            Assert.AreEqual(-10,f.Y,0.00001);
            Assert.AreEqual(-10,f.Z,0.00001);
        }

        [TestMethod()]
        public void GetKMuForceTest4() {
            var me = new VelPosDummy(0,0,0,0,7,77);
            var toThis = new VelPosDummy(7,0,0,0,0,0);

            var diag = 7;

            var f = Phys3DHelper.GetKMuForce(me,toThis,10,10,1 * diag);
            Assert.AreEqual(0,f.X,0.00001);
            Assert.AreEqual(0,f.Y,0.00001);
            Assert.AreEqual(0,f.Z,0.00001);
        }

        [TestMethod()]
        public void GetKMuForceTest5() {
            var me = new VelPosDummy(0,0,0,0,7,77);
            var toThis = new VelPosDummy(7,0,0,0,-99,199);

            var diag = 7;

            var f = Phys3DHelper.GetKMuForce(me,toThis,10,10,1 * diag);
            Assert.AreEqual(0,f.X,0.00001);
            Assert.AreEqual(0,f.Y,0.00001);
            Assert.AreEqual(0,f.Z,0.00001);
        }

        [TestMethod()]
        public void GetKMuForceTest6() {
            var me = new VelPosDummy(0,0,0,1,7,77);
            var toThis = new VelPosDummy(7,0,0,0,-99,199);

            var diag = 7;

            var f = Phys3DHelper.GetKMuForce(me,toThis,10,10,1 * diag);
            Assert.AreEqual(-10,f.X,0.00001);
            Assert.AreEqual(0,f.Y,0.00001);
            Assert.AreEqual(0,f.Z,0.00001);
        }

        [TestMethod()]
        public void GetKMuForceTest7() {
            var me = new VelPosDummy(0,0,0,1,7,77);
            var toThis = new VelPosDummy(7,0,0,-1,-99,199);

            var diag = 7;

            var f = Phys3DHelper.GetKMuForce(me,toThis,10,10,1 * diag);
            Assert.AreEqual(-20,f.X,0.00001);
            Assert.AreEqual(0,f.Y,0.00001);
            Assert.AreEqual(0,f.Z,0.00001);
        }

        [TestMethod()]
        public void GetKMuForceTest8() {
            var me = new VelPosDummy(0,0,0,-1,7,77);
            var toThis = new VelPosDummy(7,0,0,1,-99,199);

            var diag = 7;

            var f = Phys3DHelper.GetKMuForce(me,toThis,10,10,1 * diag);
            Assert.AreEqual(20,f.X,0.00001);
            Assert.AreEqual(0,f.Y,0.00001);
            Assert.AreEqual(0,f.Z,0.00001);
        }

        [TestMethod()]
        public void GetKMuForceTest9() {
            var me = new VelPosDummy(0,0,0,-1,7,77);
            var toThis = new VelPosDummy(7,0,0,1,-99,199);

            var diag = 6;

            var f = Phys3DHelper.GetKMuForce(me,toThis,100,10,1 * diag);
            Assert.AreEqual(20 + 100,f.X,0.00001);
            Assert.AreEqual(0,f.Y,0.00001);
            Assert.AreEqual(0,f.Z,0.00001);
        }
    }
}