using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPH_3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPH_3D.Tests {
    [TestClass()]
    public class Sph3DTests {
        class ParticleDummy : Particle3DBase {
            public ParticleDummy(double hmax) : base(hmax) {
            }

            public override int StuffCount {
                get {
                    return 1;
                }
            }

            public override void DoStuff(int stuffIndex) {
                //
            }
        }

        [TestMethod()]
        public void Sph3DTest() {
            var part = new List<ParticleDummy>();
            double hmax = 0.499;
            double shagX = 0.25, shagY = 0.25, shagZ = 0.25;
            for(int i = 0; i < 20; i++) {
                for(int j = 0; j < 20; j++) {
                    for(int k = 0; k < 10; k++) {
                        var p = new ParticleDummy(hmax);
                        p.X = i * shagX;
                        p.Y = j * shagY;
                        p.Z = k * shagZ;
                        // p.Name = $"p{i * j}";
                        part.Add(p);
                    }

                }
            }
            var wall = new List<ParticleDummy>();
            for(int i = 0; i < 20; i++) {
                for(int j = 0; j < 3; j++) {
                    for(int k = 0; k < 10; k++) {
                        var p = new ParticleDummy(hmax);
                        p.X = i * shagX;
                        p.Y = -shagY - j * shagY;
                        p.Z = k * shagZ;
                        // p.Name = $"w{i * j}";
                        wall.Add(p);
                    }

                }
            }


            var sph = new Sph3D(part,wall);
            var v0 = sph.Rebuild();
            int Mb = sizeof(double)*v0.Length /1024;

            var v1 = v0 /44d + 44d*v0;
            sph.FillCells();
            sph.FillNeibs();

            var maxNeibs = sph.AllParticles.Max(p => p.Neibs.Where(n => p.GetDistTo(n) < hmax).Count());
            var minNeibs = sph.AllParticles.Min(p => p.Neibs.Where(n => p.GetDistTo(n) < hmax).Count());

            var gr = from p in sph.AllParticles
                     group p by p.Neibs.Where(n => p.GetDistTo(n) < hmax).Count() into groups
                     select new {
                         groups.Key,
                         N = groups.Count()
                     };
            var dict = gr.ToDictionary(g => g.Key);

            Assert.AreEqual(26,maxNeibs);
            Assert.AreEqual(7,minNeibs);

        }
    }
}