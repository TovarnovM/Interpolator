using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotSim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp3D.Math.Core;
using static System.Math;

namespace RobotSim.Tests {
    [TestClass()]
    public class RbWheelTests {


        //[TestMethod()]
        //public void GetLocalRH_wheelForceTest() {
        //    var wheel = new RbWheel(
        //        n: 10,
        //        R: 10,
        //        R_max: 15,
        //        H_wheel: 1,
        //        H_zac: 1,
        //        kR: 77,
        //        muR: 77,
        //        kH: 66,
        //        muH: 66);
        //    var f1 = wheel.GetLocalRH_wheelForce(new Vector3D(0,10,0));
        //    Assert.AreEqual(0d,f1.GetLength(),0.0000001);

        //    var p2 = new Vector3D(0,1,1).Norm * 10;
        //    var f2 = wheel.GetLocalRH_wheelForce(p2);
        //    Assert.AreEqual(0d,f2.GetLength(),0.0000001);

        //    var p3 = new Vector3D(0,1,1).Norm * 9;
        //    var f3 = wheel.GetLocalRH_wheelForce(p3);
        //    Assert.AreEqual(1 * 77d,f3.GetLength(),0.0000001);

        //    var p4 = new Vector3D(0,1,1).Norm * 9;
        //    p4.X = wheel.H_wheel * 0.5 + 1;
        //    var f4 = wheel.GetLocalRH_wheelForce(p4);
        //    Assert.AreEqual(1 * 77d,f4.GetLength(),0.0000001);

        //    var p5 = new Vector3D(0,1,1).Norm * 9;
        //    p5.X = wheel.H_wheel * 0.5 - 0.5;
        //    var f5 = wheel.GetLocalRH_wheelForce(p5);
        //    Assert.AreEqual(1 * 77d,f5.GetLength(),0.0000001);

        //    var p6 = new Vector3D(0,1,1).Norm * 10;
        //    p6.X = wheel.H_wheel * 0.5 - 0.1;  //0.4
        //    var f6 = wheel.GetLocalRH_wheelForce(p6);
        //    Assert.AreEqual(-0.4 * 66,f6.X,0.0000001);

        //    var p7 = new Vector3D(0,1,1).Norm * 9;
        //    p7.X = wheel.H_wheel * 0.5 - 0.1;  //0.4
        //    var f7 = wheel.GetLocalRH_wheelForce(p7);
        //    Assert.AreEqual(Pow(0.4 * 66,2) + Pow(77,2),f7.GetLengthSquared(),0.0000001);
        //}

        [TestMethod()]
        public void GetClosestIndTest() {
            var wheel = new RbWheel(
                n: 7,
                R: 10,
                R_max: 15,
                mass: 1,
                H_wheel: 1,
                H_zac: 1,
                kR: 77,
                muR: 77,
                kH: 66,
                muH: 66);

            wheel.SetPosition(new Vector3D(0,1,0),new Vector3D(0,1,1),new Vector3D(0,0,0), new Vector3D(1,0,0));
            

            Assert.AreEqual(6,wheel.GetClosestInd(wheel.WorldTransform_1 * new Vector3D(110,1,0)));
            Assert.AreEqual(5,wheel.GetClosestInd(wheel.WorldTransform_1 * new Vector3D(110,1,-1)));
            Assert.AreEqual(0,wheel.GetClosestInd(wheel.WorldTransform_1 * new Vector3D(110,1,1)));
            Assert.AreEqual(1,wheel.GetClosestInd(wheel.WorldTransform_1 * new Vector3D(110,0,1)));
            Assert.AreEqual(3,wheel.GetClosestInd(wheel.WorldTransform_1 * new Vector3D(110,-1,0)));
            Assert.AreEqual(4,wheel.GetClosestInd(wheel.WorldTransform_1 * new Vector3D(110,-1,-1.01)));
            Assert.AreEqual(0,wheel.GetClosestInd(wheel.WorldTransform_1 * new Vector3D(110,1,0.7)));
        }

        //[TestMethod()]
        //public void GetlocalTauForceTest() {
        //    var wheel = new RbWheel(
        //            n: 7,
        //            R: 10,
        //            R_max: 15,
        //            H_wheel: 1,
        //            H_zac: 1,
        //            kR: 77,
        //            muR: 77,
        //            kH: 66,
        //            muH: 66);

        //    wheel.Betta = 30 * PI / 180;
        //    var f = wheel.GetlocalKTauForce(new Vector3D(0.4,-10,0));
        //    var yansw = (0.2673 * 10 / 47.8570) * 66;
        //    var zansw = -(3.5664 * 10 / 47.8570) * 66;
        //    Assert.IsTrue(Vector3D.ApproxEqual(f, new Vector3D(0,yansw,zansw), 0.1));



        //}
    }
}