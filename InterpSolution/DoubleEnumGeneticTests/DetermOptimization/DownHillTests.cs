using Microsoft.VisualStudio.TestTools.UnitTesting;
using DoubleEnumGenetic.DetermOptimization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Chromosomes;

namespace DoubleEnumGenetic.DetermOptimization.Tests {
    [TestClass()]
    public class DownHillTests {
        public class fitnessTest : IFitness {
            public IList<IGeneDE> GInfo { get; set; }

            public fitnessTest() {
                GInfo = new List<IGeneDE>(2);
                GInfo.Add(new GeneDoubleRange("x",-0.7,0.7));
                GInfo.Add(new GeneDoubleRange("y",-10,10));
            }

            public ChromosomeD GetNewChromo() {
                var c = new ChromosomeD(GInfo);
                return c;

            }
            public ChromosomeD GetNewChromo(double x,double y) {
                var c = GetNewChromo();
                c["x"] = x;
                c["y"] = y;
                return c;

            }
            public double x0 = 0.5, y0 = -0.5, maximum = 77d;
            public double Evaluate(IChromosome chromosome) {
                var c = chromosome as ChromosomeD;
                if(c == null)
                    throw new Exception("Не та хромосома");
                double x = c["x"]- x0;
                double y = c["y"]+ y0;
                return maximum - x * x - y * y;
            }
        }
        [TestMethod()]
        public void OPtimizatorTest1() {
            var f = new fitnessTest();
            var c = f.GetNewChromo(-0.5,-7);
            var sm = new DownHill();
            sm.ShagNumber *= 3;
            sm.eps = 0.00000001;
            var opt = new Optimizator(c,f,sm);

            opt.Start();
            var bs = opt.BestChromosome as ChromosomeD;
            double x = bs["x"] , y = bs["y"];
            Assert.AreEqual(77d,bs.Fitness.Value,sm.eps);
            
            
        }

        [TestMethod()]
        public void OPtimizatorTest2() {
            var f = new fitnessTest();
            f.x0 = 0d;
            f.y0 = 10;
            var c = f.GetNewChromo(-0.5,-7);
            var sm = new DownHill();
            sm.ShagNumber *= 3;
            sm.eps = 0.00000001;
            var opt = new Optimizator(c,f,sm);

            opt.Start();
            var bs = opt.BestChromosome as ChromosomeD;
            double x = bs["x"], y = bs["y"];
            Assert.AreEqual(f.maximum,bs.Fitness.Value,sm.eps);


        }

        [TestMethod()]
        public void OPtimizatorTest3() {
            var f = new fitnessTest();
            f.x0 = 0d;
            f.y0 = 17;
            var c = f.GetNewChromo(-0.5,-7);
            var sm = new DownHill();
            sm.ShagNumber *= 3;
            sm.eps = 0.00000001;
            var opt = new Optimizator(c,f,sm);

            opt.Start();
            var bs = opt.BestChromosome as ChromosomeD;
            double x = bs["x"], y = bs["y"];
            Assert.AreEqual(f.x0,x,0.0001);
            Assert.AreEqual(-10,y,0.0001);

        }

        [TestMethod()]
        public void OPtimizatorTest4() {
            var f = new fitnessTest();
            f.x0 = 0d;
            f.y0 = 10;
            var c = f.GetNewChromo(-0.5,-7);
            var sm = new Nesterov();
            sm.ShagNumber *= 3;
            sm.etta = 0.5;
            sm.eps = 0.00001;
            var opt = new Optimizator(c,f,sm);

            opt.Start();
            var bs = opt.BestChromosome as ChromosomeD;
            double x = bs["x"], y = bs["y"];
            Assert.AreEqual(f.maximum,bs.Fitness.Value,sm.eps*10);

        }
    }
}