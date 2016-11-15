using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPHmain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPHmain.Tests {
    [TestClass()]
    public class Sph2DTests {
        class ParticleDummy : Particle2DBase {
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
        public void Sph2DTest() {
            var part = new List<ParticleDummy>();
            double hmax = 0.499;
            double shagX = 0.25, shagY = 0.25;
            for(int i = 0; i < 30; i++) {
                for(int j = 0; j < 20; j++) {
                    var p = new ParticleDummy(hmax);
                    p.X = i * shagX;
                    p.Y = j * shagY;
                   // p.Name = $"p{i * j}";
                    part.Add(p);
                }
            }
            var wall = new List<ParticleDummy>();
            for(int i = 0; i < 30; i++) {
                for(int j = 0; j < 5; j++) {
                    var p = new ParticleDummy(hmax);
                    p.X = i * shagX;
                    p.Y = -shagY - j * shagY;
                   // p.Name = $"w{i * j}";
                    wall.Add(p);
                }
            }


            var sph = new Sph2D(part,wall);
            sph.Rebuild();
            sph.FillCells();
            sph.FillNeibs();

            var maxNeibs = sph.AllParticles.Max(p => p.Neibs.Where(n => p.GetDistTo(n)<hmax).Count());
            var minNeibs = sph.AllParticles.Min(p => p.Neibs.Where(n => p.GetDistTo(n) < hmax).Count());

            Assert.AreEqual(8,maxNeibs);
            Assert.AreEqual(3,minNeibs);

        }
    }
}