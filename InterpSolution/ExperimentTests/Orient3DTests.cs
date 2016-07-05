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
    public class Orient3DTests {
        [TestMethod()]
        public void LocalToWorldTest() {
            var o = new Orient3D();
            o.Q = QuaternionD.FromAxisAngle(Vector3D.XAxis,45 * Math.PI / 180.0);
            var tstVec1 = Vector3D.XAxis;
            var answ1 = tstVec1 - o.Loc2Glob( tstVec1);
            Assert.AreEqual(0d,answ1.GetLength(),0.000001);

            var tstVec2 = Vector3D.YAxis;
            var answ2 = new Vector3D(0,Math.Sqrt(2.0),Math.Sqrt(2.0))/2 - o.Loc2Glob( tstVec2);
            Assert.AreEqual(0d,answ2.GetLength(),0.000001);

            var tstVec3 = Vector3D.ZAxis;
            var answ3 = new Vector3D(0,-Math.Sqrt(2.0),Math.Sqrt(2.0))/2 - o.Loc2Glob( tstVec3);
            Assert.AreEqual(0d,answ3.GetLength(),0.000001);


            o.Q = QuaternionD.FromAxisAngle(Vector3D.YAxis,45 * Math.PI / 180.0);
            tstVec1 = Vector3D.YAxis;
            answ1 = tstVec1 - o.Loc2Glob( tstVec1);
            Assert.AreEqual(0d,answ1.GetLength(),0.000001);

            tstVec2 = Vector3D.XAxis;
            answ2 = new Vector3D(Math.Sqrt(2.0),0,-Math.Sqrt(2.0)) / 2 - o.Loc2Glob( tstVec2);
            Assert.AreEqual(0d,answ2.GetLength(),0.000001);

            tstVec3 = Vector3D.ZAxis;
            answ3 = new Vector3D(Math.Sqrt(2.0),0,Math.Sqrt(2.0)) / 2 - o.Loc2Glob( tstVec3);
            Assert.AreEqual(0d,answ3.GetLength(),0.000001);

            o.Q = QuaternionD.FromAxisAngle(Vector3D.ZAxis,45 * Math.PI / 180.0);
            tstVec1 = Vector3D.ZAxis;
            answ1 = tstVec1 - o.Loc2Glob( tstVec1);
            Assert.AreEqual(0d,answ1.GetLength(),0.000001);

            tstVec2 = Vector3D.XAxis;
            answ2 = new Vector3D(Math.Sqrt(2.0),Math.Sqrt(2.0),0) / 2 - o.Loc2Glob( tstVec2);
            Assert.AreEqual(0d,answ2.GetLength(),0.000001);

            tstVec3 = Vector3D.YAxis;
            answ3 = new Vector3D(-Math.Sqrt(2.0),Math.Sqrt(2.0),0) / 2 - o.Loc2Glob( tstVec3);
            Assert.AreEqual(0d,answ3.GetLength(),0.000001);
        }

        [TestMethod()]
        public void WorldToLocalTest() {
            var o = new Orient3D();
            var two = Math.Sqrt(2.0) * 0.5;
            o.Q = QuaternionD.FromAxisAngle(new Vector3D(1,1,1),77 * Math.PI / 180.0);

            var tstVec1 = new Vector3D(4,4,4);
            var answ1 = tstVec1 - o.Glob2Loc( tstVec1);
            Assert.AreEqual(0d,answ1.GetLength(),0.000001);

            o.Q = QuaternionD.FromAxisAngle(Vector3D.XAxis,-45 * Math.PI / 180.0);
  
            var tstVec2 = Vector3D.YAxis;
            var answ2 = new Vector3D(0,two,two) - o.Glob2Loc( tstVec2);
            Assert.AreEqual(0d,answ2.GetLength(),0.000001);

            var tstVec3 = Vector3D.ZAxis;
            var answ3 = new Vector3D(0,-two,two) - o.Glob2Loc( tstVec3);
            Assert.AreEqual(0d,answ3.GetLength(),0.000001);

            o.Q = QuaternionD.FromAxisAngle(Vector3D.ZAxis,45 * Math.PI / 180.0);

            tstVec2 = Vector3D.XAxis;
            answ2 = new Vector3D(two,-two,0) - o.Glob2Loc( tstVec2);
            Assert.AreEqual(0d,answ2.GetLength(),0.000001);

            tstVec3 = Vector3D.YAxis;
            answ3 = new Vector3D(two,two,0) - o.Glob2Loc( tstVec3);
            Assert.AreEqual(0d,answ3.GetLength(),0.000001);

        }

    }
}