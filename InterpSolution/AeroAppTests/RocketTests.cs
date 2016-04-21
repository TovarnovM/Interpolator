using Microsoft.VisualStudio.TestTools.UnitTesting;
using RocketAero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RocketAero.Tests
{
    [TestClass()]
    public class RocketTests
    {
        private Rocket _r;

        [TestMethod(), TestInitialize()]
        public void DefaultRocketTest()
        {
            Rocket r = new Rocket();
            var ag = new AeroGraphs();
            r.AeroGr = ag;

            r.Body = new RocketBody(ag)
            {
                L = 4.18,
                L_nos = 0.356,
                L_korm = 0.09,
                D = 0.31,
                D1 = 0.28,
                //Nose = new RocketNos_ConePlusCyl()
                Nose = new RocketNos_Compose("7_2", 118 * 2 / 310.0)
            };

            r.W_I = new WingOrient(
                new RocketWing(ag)
                {
                    B0 = 1.022,
                    B1 = 0.327,
                    L = 1.016,
                    Hi0 = 50,
                    C_shtr = 0.07,
                    D = r.Body.D,
                    Profile = new WingProf_6(0.4)
                })
            {
                X = 1.82028
            };
            r.W_II = new WingOrient(
                new RocketWing(ag)
                {
                    B0 = 0.55,
                    B1 = 0.07117,
                    L = 0.7,
                    Hi0 = 50,
                    C_shtr = 0.072,
                    D = r.Body.D,
                    Profile = new WingProf_6(0.4)
                })
            {
                X = 3.599999,
                X_povor_otn = 218.0 / 338.0
            };
            _r = r;
        }

        [TestMethod()]
        public void GetCyTest_body()
        {
            Assert.AreEqual(0.0344, _r.Body.GetCy1a(0.3), 0.001);
            Assert.AreEqual(0.0346, _r.Body.GetCy1a(0.8), 0.001);
            Assert.AreEqual(0.0367, _r.Body.GetCy1a(1), 0.001);
            Assert.AreEqual(0.042, _r.Body.GetCy1a(1.2), 0.001);
            Assert.AreEqual(0.0473, _r.Body.GetCy1a(2.0), 0.001);
            Assert.AreEqual(0.046, _r.Body.GetCy1a(2.9), 0.001);
        }

        [TestMethod()]
        public void GetCy_a_Test_wingz_pure()
        {
            Assert.AreEqual(0.0299, _r.W_I.Wing.GetCy1a(0.3), 0.001);
            Assert.AreEqual(0.0326, _r.W_I.Wing.GetCy1a(0.8), 0.001);
            Assert.AreEqual(0.0372, _r.W_I.Wing.GetCy1a(1), 0.001);
            Assert.AreEqual(0.0373, _r.W_I.Wing.GetCy1a(1.2), 0.001);
            Assert.AreEqual(0.0289, _r.W_I.Wing.GetCy1a(2), 0.001);
            Assert.AreEqual(0.0224, _r.W_I.Wing.GetCy1a(2.9), 0.001);

            Assert.AreEqual(0.0405, _r.W_II.Wing.GetCy1a(0.3), 0.001);
            Assert.AreEqual(0.0459, _r.W_II.Wing.GetCy1a(0.8), 0.001);
            Assert.AreEqual(0.0536, _r.W_II.Wing.GetCy1a(1), 0.001);
            Assert.AreEqual(0.0495, _r.W_II.Wing.GetCy1a(1.2), 0.001);
            Assert.AreEqual(0.0336, _r.W_II.Wing.GetCy1a(2), 0.001);
            Assert.AreEqual(0.026, _r.W_II.Wing.GetCy1a(2.9), 0.001);
        }

        [TestMethod()]
        public void GetCy_a_Test_wingz()
        {
            _r.M = 0.3;
            Assert.AreEqual(0.053, _r.Cy1a_I, 0.001);
            _r.M = 0.8;
            Assert.AreEqual(0.058, _r.Cy1a_I, 0.001);
            _r.M = 1.0;
            Assert.AreEqual(0.067, _r.Cy1a_I, 0.001);
            _r.M = 1.2;
            Assert.AreEqual(0.067, _r.Cy1a_I, 0.001);
            _r.M = 2.0;
            Assert.AreEqual(0.05, _r.Cy1a_I, 0.001);
            _r.M = 2.9;
            Assert.AreEqual(0.035, _r.Cy1a_I, 0.001);

            _r.M = 0.3;
            Assert.AreEqual(0.037, _r.Cy1a_II, 0.001);
            _r.M = 0.8;
            Assert.AreEqual(0.037, _r.Cy1a_II, 0.001);
            _r.M = 1.0;
            Assert.AreEqual(0.031, _r.Cy1a_II, 0.001);
            _r.M = 1.2;
            Assert.AreEqual(0.0264, _r.Cy1a_II, 0.001);
            _r.M = 2.0;
            Assert.AreEqual(0.0215, _r.Cy1a_II, 0.001);
            _r.M = 2.9;
            Assert.AreEqual(0.019, _r.Cy1a_II, 0.001);
        }

        [TestMethod()]
        public void CxiTest()
        {
            //var xi = new double[,]
            //    { { 0.162,   0.16,    0.251 ,  0.398  , 0.482 ,  0.476 },
            //    {0.092,   0.091,   0.142,   0.227 ,  0.275 ,  0.271},
            //    {0.041 ,  0.04 ,   0.064 ,  0.101 ,  0.123 ,  0.122},
            //    {0.01 ,   0.01 ,   0.016,   0.025 ,  0.031 ,  0.031},
            //    {0 , 0  , 0 ,  0   ,0  , 0},
            //    {0.01 ,   0.01  ,  0.016  , 0.025 ,  0.031  , 0.031},
            //    {0.041 ,  0.04  ,  0.064 ,  0.101 ,  0.123  , 0.122},
            //    {0.092 ,  0.091  , 0.142 ,  0.227 ,  0.275  , 0.271},
            //    {0.162 ,  0.16  ,  0.251 ,  0.398 ,  0.482  , 0.476},
            //    {0.252  , 0.249 ,  0.388  , 0.613,   0.742  , 0.733 } };
            var xi = new double[10, 6];

            var alph = new double[] { -20, -15, -10, -5, 0, 5, 10, 15, 20, 25 };
            var m = new double[] { 0.3, 0.8, 1.0, 1.2, 2.0, 2.9 };
            for (int i = 0; i < 10; i++)
            {
                _r.Alpha = alph[i];
                for (int j = 0; j < 6; j++)
                {
                    _r.M = m[j];
                    xi[i, j] = _r.Cxi_II;
                    //Assert.AreEqual(xi[i, j], _r.Cxi_f, xi[i, j]*0.1);
                }
            }
            var ee = 1;


        }

        [TestMethod()]
        public void CxNoseTest()
        {
            Assert.AreEqual(0.0344, _r.Body.Cx_nose(0.3), 0.001);
            Assert.AreEqual(0.07, _r.Body.Cx_nose(0.8), 0.001);
            Assert.AreEqual(0.205, _r.Body.Cx_nose(1), 0.001);
            Assert.AreEqual(0.317, _r.Body.Cx_nose(1.2), 0.001);
            Assert.AreEqual(0.469, _r.Body.Cx_nose(2), 0.001);
            Assert.AreEqual(0.505, _r.Body.Cx_nose(2.9), 0.001);
        }

        [TestMethod()]
        public void Cx0BodyTest()
        {
            _r.M = 0.3;
            Assert.AreEqual(0.208, _r.Cx0_f, 0.001);
            _r.M = 0.8;
            Assert.AreEqual(0.225, _r.Cx0_f, 0.001);
            _r.M = 1;
            Assert.AreEqual(0.368, _r.Body.Cx0(1), 0.001);
            _r.M = 1.2;
            Assert.AreEqual(0.476, _r.Body.Cx0(1.2), 0.001);
            _r.M = 2.0;
            Assert.AreEqual(0.59, _r.Body.Cx0(2), 0.001);
            _r.M = 2.9;
            Assert.AreEqual(0.595, _r.Body.Cx0(2.9), 0.001);
        }

        [TestMethod()]
        public void Cx0WingyTest()
        {
            _r.M = 0.3;
            Assert.AreEqual(0.00496, _r.Cx1_I, 0.001);
            _r.M = 0.8;
            Assert.AreEqual(0.0045, _r.Cx1_I, 0.001);
            _r.M = 1;
            Assert.AreEqual(0.0042, _r.Cx1_I, 0.001);
            _r.M = 1.2;
            Assert.AreEqual(0.0165, _r.Cx1_I, 0.001);
            _r.M = 2.0;
            Assert.AreEqual(0.0126, _r.Cx1_I, 0.001);
            _r.M = 2.9;
            Assert.AreEqual(0.0093, _r.Cx1_I, 0.001);

            _r.M = 0.3;
            Assert.AreEqual(0.0062, _r.Cx1_II, 0.001);
            _r.M = 0.8;
            Assert.AreEqual(0.0054, _r.Cx1_II, 0.001);
            _r.M = 1;
            Assert.AreEqual(0.0051, _r.Cx1_II, 0.001);
            _r.M = 1.2;
            Assert.AreEqual(0.0222, _r.Cx1_II, 0.001);
            _r.M = 2.0;
            Assert.AreEqual(0.0156, _r.Cx1_II, 0.001);
            _r.M = 2.9;
            Assert.AreEqual(0.0107, _r.Cx1_II, 0.001);
        }

        [TestMethod()]
        public void Get_Kaa_starTest()
        {
            Assert.AreEqual(1.83, _r.Get_Kaa_star(_r.W_I), 0.001);
            Assert.AreEqual(2.277, _r.Get_Kaa_star(_r.W_II), 0.001);
        }

        [TestMethod()]
        public void Get_kaa_starTest()
        {
            Assert.AreEqual(1.36, _r.Get_kaa_star(_r.W_I), 0.001);
            Assert.AreEqual(1.527, _r.Get_kaa_star(_r.W_II), 0.001);
        }

        [TestMethod()]
        public void Get_KaaTest()
        {
            _r.M = 0.3;
            Assert.AreEqual(1.787, _r.Kaa_I, 0.01);
            Assert.AreEqual(2.175, _r.Kaa_II, 0.01);

            _r.M = 1.2;
            Assert.AreEqual(1.787, _r.Kaa_I, 0.01);
            Assert.AreEqual(2.098, _r.Kaa_II, 0.01);

            _r.M = 2.9;
            Assert.AreEqual(1.555, _r.Kaa_I, 0.01);
            Assert.AreEqual(1.440, _r.Kaa_II, 0.01);
        }

        [TestMethod()]
        public void Get_kaaTest()
        {
           _r.M = 2;
            Assert.AreEqual(1.26, _r.kaa_I, 0.01);
            Assert.AreEqual(1.377, _r.kaa_II, 0.01);
        }

        [TestMethod()]
        public void Get_kd0Test()
        {
            _r.M = 0.3;
            Assert.AreEqual(0.99, _r.kd0_I, 0.01);
            Assert.AreEqual(0.96, _r.kd0_II, 0.01);

            _r.M = 1.2;
            Assert.AreEqual(0.98, _r.kd0_I, 0.01);
            Assert.AreEqual(0.96, _r.kd0_II, 0.01);

            _r.M = 2.9;
            Assert.AreEqual(0.85, _r.kd0_I, 0.01);
            Assert.AreEqual(0.826, _r.kd0_II, 0.01);
        }

        [TestMethod()]
        public void Get_Kd0Test()
        {
            _r.M = 0.3;
            Assert.AreEqual(1.33, _r.Kd0_I, 0.01);
            Assert.AreEqual(1.43, _r.Kd0_II, 0.01);

            _r.M = 1.2;
            Assert.AreEqual(1.33, _r.Kd0_I, 0.01);
            Assert.AreEqual(1.38, _r.Kd0_II, 0.01);

            _r.M = 2.9;
            Assert.AreEqual(1.15, _r.Kd0_I, 0.01);
            Assert.AreEqual(0.94, _r.Kd0_II, 0.01);
        }

        [TestMethod()]
        public void Get_kt_I_shtrTest()
        {
            Assert.AreEqual(0.999, _r.Get_kt_I_shtr(0.3, 0.0), 0.001);
            Assert.AreEqual(0.997, _r.Get_kt_I_shtr(0.8, 0.0), 0.001);
            Assert.AreEqual(1.0, _r.Get_kt_I_shtr(1, 0.0), 0.001);
            Assert.AreEqual(0.996, _r.Get_kt_I_shtr(1.2, 0.0), 0.001);
            Assert.AreEqual(0.99, _r.Get_kt_I_shtr(2.0, 0.0), 0.001);
            Assert.AreEqual(0.976, _r.Get_kt_I_shtr(2.9, 0.0), 0.001);
            Assert.AreEqual(0.958, _r.Get_kt_I_shtr(3.5, 0.0), 0.001);
        }

        [TestMethod()]
        public void Get_kt_II_shtrTest()
        {
            Assert.AreEqual(0.99, _r.Get_kt_II_shtr(0.3, 0.0), 0.001);
            Assert.AreEqual(0.985, _r.Get_kt_II_shtr(0.8, 0.0), 0.001);
            Assert.AreEqual(0.979, _r.Get_kt_II_shtr(1, 0.0), 0.001);
            Assert.AreEqual(0.967, _r.Get_kt_II_shtr(1.2, 0.0), 0.001);
            Assert.AreEqual(0.933, _r.Get_kt_II_shtr(2.0, 0.0), 0.001);
            Assert.AreEqual(0.922, _r.Get_kt_II_shtr(2.9, 0.0), 0.001);
            Assert.AreEqual(0.920, _r.Get_kt_II_shtr(3.5, 0.0), 0.001);
        }

        [TestMethod()]
        public void Get_eps_sr_alphaTest()
        {
            _r.M = 0.3;
            Assert.AreEqual(0.575, _r.Get_eps_sr_alpha(), 0.01);
            _r.M = 0.8;
            Assert.AreEqual(0.62, _r.Get_eps_sr_alpha(), 0.01);
            _r.M = 1;
            Assert.AreEqual(0.72, _r.Get_eps_sr_alpha(), 0.01);
            _r.M = 1.2;
            Assert.AreEqual(0.73, _r.Get_eps_sr_alpha(), 0.01);
            _r.M = 2.0;
            Assert.AreEqual(0.63, _r.Get_eps_sr_alpha(), 0.01);
            _r.M = 2.9;
            Assert.AreEqual(0.499, _r.Get_eps_sr_alpha(), 0.01);
        }

        [TestMethod()]
        public void Get_eps_sr_deltaTest()
        {
            _r.M = 0.3;
            Assert.AreEqual(0.35, _r.Get_eps_sr_delta(), 0.01);
            _r.M = 0.8;
            Assert.AreEqual(0.4, _r.Get_eps_sr_delta(), 0.01);
            _r.M = 1;
            Assert.AreEqual(0.46, _r.Get_eps_sr_delta(), 0.01);
            _r.M = 1.2;
            Assert.AreEqual(0.48, _r.Get_eps_sr_delta(), 0.01);
            _r.M = 2.0;
            Assert.AreEqual(0.44, _r.Get_eps_sr_delta(), 0.01);
            _r.M = 2.9;
            Assert.AreEqual(0.36, _r.Get_eps_sr_delta(), 0.01);
        }



    }
}