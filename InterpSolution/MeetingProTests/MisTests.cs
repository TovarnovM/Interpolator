using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeetingPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sharp3D.Math.Core;
using Microsoft.Research.Oslo;

namespace MeetingPro.Tests {
    [TestClass()]
    public class MisTests {
        Mis mis;

        [TestMethod(), TestInitialize()]
        public void SqrTest() {
            var d = 9;
            mis = new Mis();
            Assert.AreEqual(81d, Mis.Sqr(d));
        }

        [TestMethod()]
        public void Synch_0Test() {
            mis.Synch_0(1);
        }

        [TestMethod()]
        public void Synch_1Test() {
            mis.Synch_0(1);
            mis.Synch_1();
        }

        [TestMethod()]
        public void Synch_2Test() {
            mis.Synch_0(1);
            mis.Synch_1();
            mis.Synch_2();
        }

        [TestMethod()]
        public void Delta_Test() {
            mis.delta_i_rad[0] = 15;
            mis.delta_i_rad[1] = -15;
            mis.delta_i_rad[2] = 5;
            mis.delta_i_rad[3] = -10;
            var answ = mis.matr_lambda * mis.delta_i_rad;
        }

        [TestMethod()]
        public void Vector_Test() {
            mis.Vel.X = 200;
            mis.Vel.Y = 30;
            mis.Vel.Z = -20;
            var v0 = mis.Rebuild(10);
            var a0 = mis.f(10, v0);
            var s = mis.GetDiffPrms().Select(dp => dp.FullName).ToArray();
            foreach (var d in v0.ToArray().Concat(a0.ToArray())) {
                Assert.IsFalse(Double.IsNaN(d));
            }
        }

        [TestMethod()]
        public void SetNDemVecTest() {
            var m = new Mis();
            m.Vec3D = new Vector3D(100, 200, 300);
            m.Vel.Vec3D = new Vector3D(10, 30, 20);
            m.Omega.Y = 33;
            m.Omega.Z = -77;
            m.SetTimeSynch(11);
            m.SetPosition_LocalPoint_LocalMoveToIt_LocalFixed(new Vector3D(1, 0, 0), new Vector3D(1, 1, 1), new Vector3D(0, 0, 0));
            m.SynchQandM();

            var ndv = m.GetNDemVec();
            var pos = m.GetMTPos();

            var m2 = new Mis();
            //m2.SetMTpos(pos);
            m2.Vel.Vec3D = new Vector3D(10, 0, 0);
            m2.SetNDemVec(ndv);

            var ndv2 = m2.GetNDemVec();
            var v1 = ndv.ToVec();
            var v2 = ndv2.ToVec();
            Assert.IsTrue(v1.Equals(v2, 1E-4));
        }

        [TestMethod()]
        public void GetNDemVecTest() {
            var m = new Mis();
            m.Vec3D = new Vector3D(0, 0, 0);
            m.Vel.Vec3D = new Vector3D(10, 10 ,0);
            m.Omega.Y = 33;
            m.Omega.Z = -77;
            m.SetTimeSynch(11);
            
            m.SynchQandM();

            var ndv = m.GetNDemVec();
            Assert.IsTrue(ndv.Alpha < 0);
        }

        [TestMethod()]
        public void GetNDemVecTest2() {
            var m = new Mis();
            m.Vec3D = new Vector3D(0, 0, 0);
            m.Vel.Vec3D = new Vector3D(10, 10, 0);
            m.Omega.Y = 33;
            m.Omega.Z = -77;
            m.SetTimeSynch(11);
            m.SynchQandM();

            var ndv = m.GetNDemVec();
            Assert.IsTrue(ndv.Thetta > 0);
        }

        [TestMethod()]
        public void GetNDemVecTest33() {
            var m = new List<int> {
                1,2,3,4,1,2,5,6
            };
            var gb = m.GroupBy(i => i).ToList();
            var gb0 = gb[0];
        }
        [TestMethod()]
        public void GetNDemVecTest34() {
            var m = new List<int>();
            int n = 16;
            for (int i = 0; i < 32; i++) {
                int answ = (i % (2 * n))/n;
                m.Add(answ);
            }
            int y = 99;
        }
    }
}