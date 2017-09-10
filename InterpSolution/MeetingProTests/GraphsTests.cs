using Microsoft.VisualStudio.TestTools.UnitTesting;
using MeetingPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingPro.Tests {
    [TestClass()]
    public class GraphsTests {
        [TestMethod(), TestInitialize()]
        public void CopyTest() {
            Graphs.FilePath = @"C:\Users\User\Documents\data.xml";

            var gs = Graphs.GetNew();
            var names = gs.Names;
            var allNames = names.Aggregate("", (s1, s2) => s1 + s2 + "\n");

            Assert.IsNotNull(names);
        }
        [TestMethod()]
        public void R_rd_tst() {
            var p = Graphs.Instance["r_rd"].GetV(1, 30);
            Assert.IsTrue(p > 0);
        }

        //x_m
        [TestMethod()]
        public void X_m_tst() {
            var p = Graphs.Instance["x_m"].GetV(2.66, 18.72);
            Assert.AreEqual(1067.0, p , 0.01);
        }
        //m
        [TestMethod()]
        public void M_tst() {
            var p = Graphs.Instance["m"].GetV(2.66, 18.72);
            Assert.AreEqual(82.558, p, 0.01);
        }
        //i_x
        [TestMethod()]
        public void Ix_tst() {
            var p = Graphs.Instance["i_x"].GetV(2.66, 18.72);
            Assert.AreEqual(0.64425, p, 0.00001);
        }
        //i_yz
        [TestMethod()]
        public void Iyz_tst() {
            var p = Graphs.Instance["i_yz"].GetV(2.66, 18.72);
            Assert.AreEqual(27.039, p, 0.001);
        }
        //c_x
        [TestMethod()]
        public void Cx_tst() {
            var p = Graphs.Instance["c_x"].GetV(16, 0.9);
            Assert.AreEqual(0.717, p, 0.01);
        }
        //c_k_y
        //x_k_d
        //x_kr_d
        //c_kr_y_i
        [TestMethod()]
        public void C_kr_y_i_tst() {
            var p = Graphs.Instance["c_kr_y_i"].GetV(25, 1.05);
            Assert.AreEqual(1.752, p, 0.001);
        }
        //alpha_sk
        //m_omegaz_z_dempf
        //r_rd
        //deltam_rd
        //r_md
        //deltam_md
        //c_r_x
        [TestMethod()]
        public void C_r_x_tst() {
            var p = Graphs.Instance["c_r_x"].GetV(0, 1.05, 15);
            Assert.AreEqual(0.034, p, 0.001);
        }
        //c_r_y_i
        [TestMethod()]
        public void C_r_y_i_tst() {
            var p = Graphs.Instance["c_r_y_i"].GetV(0, 1.05, 15);
            Assert.AreEqual(0.168, p, 0.001);
        }
        //m_x0
        [TestMethod()]
        public void M_x0_tst() {
            var p = Graphs.Instance["m_x0"].GetV(3, 1.05, 10);
            Assert.AreEqual(-0.025, p, 0.001);
        }
        //ro
        //a
        //m_omegax_x_dempf
    }
}