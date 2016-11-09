using Microsoft.Research.Oslo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OneDemSPH;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDemSPH.Tests {
    [TestClass()]
    public class OneDemExampleTests {
        [TestMethod()]
        public void OneDemExampleTest() {
            var s = new OneDemExample();
            double RoTst = s.AllParticles.Sum(p =>
                p.M * KernelF.W(s.AllParticles[50].X - p.X,s.h));
            Assert.AreEqual(1.0,RoTst,0.00001);
        }
        [TestMethod]
        public void Nevermind() {
            var s = new OneDemExample();
            var lstfail = new List<Tuple<double,double>>(s.Particles.Count);
            var lstRo = new List<Tuple<double,double>>(s.Particles.Count);
            s.h /= 2;
            foreach(var particle in s.Particles) {
                var ro = s.AllParticles.Sum(p => p.M * KernelF.W(particle.X - p.X,s.h));
                lstRo.Add(new Tuple<double,double>(particle.X,ro));
                if(Math.Abs(particle.Ro - ro) > 0.0001)
                    lstfail.Add(new Tuple<double,double>(particle.X,ro));
            }

            Assert.AreEqual(1,1,0.1);
        }

        [TestMethod]
        public void KernelTest2() {
            double h = 10;
            Func<double,Vector,Vector> tstf = (t,v) => {
                var res = Vector.Zeros(1);
                res[0] = KernelF.dWdr(t,h)*Math.Sign(t);
                return res;
            };

            var sol = Ode.RK45(-3 * h,Vector.Zeros(1),tstf,h/100000).SolveFromTo(-3*h,3*h);

            foreach(var sp in sol) {
                Assert.AreEqual(KernelF.W(sp.T,h),sp.X[0],0.1);
            }


        }
        [TestMethod]
        public void KernelTest3() {
            double h = 1;
            Func<double,Vector,Vector> tstf = (t,v) => {
                var res = Vector.Zeros(1);
                res[0] = KernelF.W(t,h) ;
                return res;
            };

            var sol = Ode.RK45(-3 * h,Vector.Zeros(1),tstf,0.01).SolveFromTo(-2.5 * h,2.5 * h).Last();

            Assert.AreEqual(1d,sol.X[0],0.0001);


        }
    }
}