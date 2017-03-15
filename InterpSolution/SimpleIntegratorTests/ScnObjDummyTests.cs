using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleIntegrator;
using SPH_2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIntegrator.Tests {
    [TestClass()]
    public class ScnObjDummyTests {
        [TestMethod()]
        public void SaveToDictLoadFromDictTest() {
            int N = 200;
            double L = 0.1, shag = L / N, xL = 0.5 * L;
            double d = L / N, hmax = 1.4 * 2 * d;
            double P1 = 3E4, P2 = 1E4;
            double Ro1 = 1500, Ro2 = 1200;
            double k1 = 3, k2 = 3;

            var particles = new List<IsotropicGasParticle>(N);
            for(int i = 0; i < N; i++) {
                double x = i * shag;
                particles.Add(new IsotropicGasParticle(d,hmax) {
                    X = x,
                    P = x < xL ? P1 : P2,
                    Ro = x < xL ? Ro1 : Ro2,
                    k = x < xL ? k1 : k2,
                    isWall = x < hmax * 2 || x > (L - 2 * hmax)
                });
            }


            for(int i = 0; i < 1; i++) {

                particles.ForEach(p => {
                    p.M = p.Ro * Math.Pow(p.D,1);


                });

                particles.ForEach(p => {
                    p.Ro = particles.Sum(n => IsotropicGasParticle.W_func(n.GetDistTo(p),p.alpha * (p.D + n.D) * 0.5) * n.M);
                    p.E = p.P / ((p.k - 1d) * p.Ro);
                });
            }
            var masses = particles.Select(p => p.M).ToArray();
            var sph = new Sph2D(particles,null);

            var dict = sph.SaveToDict();


            var particles_zero = new List<IsotropicGasParticle>(N);
            for(int i = 0; i < N; i++) {
                double x = i * shag;
                particles_zero.Add(new IsotropicGasParticle(d,hmax) {
                    X = 0,
                    P = 0,
                    Ro = 0,
                    k = 0,
                    isWall = x < hmax * 2 || x > (L - 2 * hmax)
                });
            }

            var sph_zero = new Sph2D(particles_zero,null);

            sph_zero.LoadFromDict(dict);

            var dictLoaded = sph_zero.SaveToDict();

            foreach(var elem0 in dict) {
                Assert.IsTrue(dictLoaded.ContainsKey(elem0.Key));
                Assert.AreEqual(elem0.Value,dictLoaded[elem0.Key]);

            }




        }

        [TestMethod()]
        public void GetNewUniqueNameTest1() {
            string[] names = { "33","name","name2","name33", "notname33" };
            var currName = "name2";
            var answ = ScnObjDummy.GetNewUniqueName(currName,names.AsEnumerable());
            Assert.AreEqual("name34",answ);
        }

        [TestMethod()]
        public void GetNewUniqueNameTest2() {
            var pat = "pat";
            int n = 1000;
            var lst = new List<string>(n);
            for(int i = 0; i < n; i++) {
                lst.Add(ScnObjDummy.GetNewUniqueName(pat,lst));
            }

            for(int i = 1; i < n; i++) {
                Assert.AreEqual(pat + i.ToString(),lst[i]);
            }
        }
    }
}