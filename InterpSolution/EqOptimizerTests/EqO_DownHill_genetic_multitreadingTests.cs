using Microsoft.VisualStudio.TestTools.UnitTesting;
using EqOptimizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EqOptimizer.Equations;
using EqOptimizer.Data;
using EqOptimizer.Criterias;

namespace EqOptimizer.Tests {
    [TestClass()]
    public class EqO_DownHill_genetic_multitreadingTests {
        [TestMethod()]
        public void GetNewChromoTest1() {
            var eq0 = new Line_1order_eq(1);
            eq0.FillPars(new double[] { -4, 22 });
            var data = new MultyData();
            var rnd = new Random();
            double x0 = -100, x1 = 100;
            for (int i = 0; i < 10; i++) {
                var x = rnd.NextDouble() * (x1 - x0) + x0;
                var y = eq0.GetSolution(x);// + rnd.NextDouble()*0.5;
                data.Add(new OnePoint(x, y));
            }

            var eq = new Line_1order_eq(1);
            var opt = new EqO_genetic(eq, data, new MNK());
            opt.Multithread = false;
            var soleq = opt.PerformOptimization();
            Assert.AreEqual(soleq.eq["A"], -4d, 0.001);
            Assert.AreEqual(soleq.eq["B"], 22d, 0.001);

        }

        [TestMethod()]
        public void GetNewChromoTest2() {
            var rnd = new Random();
            int vc = 1;
            var eq0 = new Line_2order_eq(vc);
            double p_min = -100, p_max = 1000;
            var TruePars = eq0.Pars.Select(_ => p_min + (p_max - p_min) * rnd.NextDouble()).ToArray();
            eq0.FillPars(TruePars);
            var data = new MultyData();

            double x0 = -100, x1 = 100;
            for (int i = 0; i < eq0.ParsCount * 10; i++) {
                var x = eq0.VarNames.Select(_ => rnd.NextDouble() * (x1 - x0) + x0).ToArray(); ;
                var y = eq0.GetSolution(x);// + rnd.NextDouble()*0.5;
                data.Add(new OnePoint(answer: y, vars: x));
            }

            var eq = new Line_2order_eq(vc);
            var opt = new EqO_genetic(eq, data, new KolomSmirn(), -1000, 2000);
            var soleq = opt.PerformOptimization();
            for (int i = 0; i < eq0.ParsCount; i++) {
                Assert.AreEqual(soleq.eq[eq0.ParNames[i]], TruePars[i], (p_max - p_min) / 100);
            }

        }
        [TestMethod()]
        public void GetNewChromoTest3() {
            var rnd = new Random();
            int vc = 5;
            var eq0 = new Line_1order_eq(vc);
            double p_min = -100, p_max = 1000;
            var TruePars = eq0.Pars.Select(_ => p_min + (p_max - p_min) * rnd.NextDouble()).ToArray();
            eq0.FillPars(TruePars);
            var data = new MultyData();

            double x0 = -100, x1 = 100;
            for (int i = 0; i < eq0.ParsCount * 30; i++) {
                var x = eq0.VarNames.Select(_ => rnd.NextDouble() * (x1 - x0) + x0).ToArray(); ;
                var y = eq0.GetSolution(x);// + rnd.NextDouble()*0.5;
                data.Add(new OnePoint(answer: y, vars: x));
            }

            var eq = new Line_1order_eq(vc);
            var opt = new EqO_genetic(eq, data, new MNK(), -1000, 2000);
            var soleq = opt.PerformOptimization();
            for (int i = 0; i < eq0.ParsCount; i++) {
                Assert.AreEqual(soleq.eq[eq0.ParNames[i]], TruePars[i], (p_max - p_min) / 100*5);
            }

        }

        //[TestMethod()]
        //public void GetNewChromoTest4() {
        //    //return;
        //    var rnd = new Random();
        //    int vc = 5;
        //    var eq0 = new Line_2order_eq(vc);
        //    double p_min = 0, p_max = 1;
        //    //var TruePars = eq0.Pars.Select(_ => p_min + (p_max - p_min) * rnd.NextDouble()).ToArray();
        //    var TruePars = Enumerable.Range(1, eq0.ParsCount).Select(i => (double)i).ToArray();
        //    eq0.FillPars(TruePars);
        //    var data = new MultyData();

        //    double x0 = -1, x1 = 1;
        //    for (int i = 0; i < eq0.ParsCount * 10; i++) {
        //        var x = eq0.VarNames.Select(_ => rnd.NextDouble() * (x1 - x0) + x0).ToArray(); ;
        //        var y = eq0.GetSolution(x);// + rnd.NextDouble()*0.5;
        //        data.Add(new OnePoint(answer: y, vars: x));
        //    }

        //    var eq = new Line_2order_eq(vc);
        //    var opt = new EqO_genetic(eq, data, new MNK(), -300, 300);
        //    opt.Multithread = false;
        //    opt.Popsize = 100;

        //    var soleq = opt.PerformOptimization();

        //    var diffs = eq0.Pars.Zip(soleq.eq.Pars, (e0, e1) => e0 - e1).ToArray();
        //    var maxDiffX = data.MaxDiffVars;
        //    var maxDiffY = data.MaxDiffY;

        //    var opt2 = new EqO_downhill(eq, data, new MNK(), -300, 300);
        //    opt2.Multithread = false;
        //    soleq = opt2.PerformOptimization();


        //    for (int i = 0; i < eq0.ParsCount; i++) {
        //        Assert.AreEqual(soleq.eq[eq0.ParNames[i]], TruePars[i], (p_max - p_min) / 100*30);
        //    }

        //}

        [TestMethod()]
        public void GetNewChromoTest5() {
            //return;
            var rnd = new Random();
            int vc = 5;
            var eq0 = new Line_2order_eq(vc);
            double p_min = 0, p_max = 1;
            //var TruePars = eq0.Pars.Select(_ => p_min + (p_max - p_min) * rnd.NextDouble()).ToArray();
            var TruePars = Enumerable.Range(1, eq0.ParsCount).Select(i => (double)i).ToArray();
            eq0.FillPars(TruePars);
            var data = new MultyData();

            double x0 = -1, x1 = 1;
            for (int i = 0; i < eq0.ParsCount * 10; i++) {
                var x = eq0.VarNames.Select(_ => rnd.NextDouble() * (x1 - x0) + x0).ToArray(); ;
                var y = eq0.GetSolution(x);// + rnd.NextDouble()*0.5;
                data.Add(new OnePoint(answer: y, vars: x));
            }

            var eq = new Line_2order_eq(vc);
            //var opt = new EqO_genetic(eq, data, new MNK(), -300, 300);
            //opt.Multithread = false;
            //opt.Popsize = 100;

            //var soleq = opt.PerformOptimization();

            //var diffs = eq0.Pars.Zip(soleq.eq.Pars, (e0, e1) => e0 - e1).ToArray();
            //var maxDiffX = data.MaxDiffVars;
            //var maxDiffY = data.MaxDiffY;

            var opt2 = new EqO_downhill(eq, data, new MNK(), -300, 300);
            opt2.Multithread = false;
            
            var soleq = opt2.PerformOptimization();


            for (int i = 0; i < eq0.ParsCount; i++) {
                Assert.AreEqual(soleq.eq[eq0.ParNames[i]], TruePars[i], (p_max - p_min) / 100 * 30);
            }

        }

        [TestMethod()]
        public void GetNewChromoTest6() {
            //return;
            var rnd = new Random();
            int vc = 5;
            var eq0 = new Line_2order_eq(vc);
            double p_min = 0, p_max = 1;
            var TruePars = eq0.Pars.Select(_ => p_min + (p_max - p_min) * rnd.NextDouble()).ToArray();
            //var TruePars = Enumerable.Range(1, eq0.ParsCount).Select(i => (double)i).ToArray();
            eq0.FillPars(TruePars);
            var data = new MultyData();

            double x0 = -1, x1 = 1;
            for (int i = 0; i < eq0.ParsCount * 30; i++) {
                var x = eq0.VarNames.Select(_ => rnd.NextDouble() * (x1 - x0) + x0).ToArray(); ;
                var y = eq0.GetSolution(x);// + rnd.NextDouble()*0.5;
                data.Add(new OnePoint(answer: y, vars: x));
            }

            var eq = new Line_2order_eq(vc);


            var opt2 = new EqO_downhill(eq, data, new MNK(), -300, 300);
            opt2.Multithread = false;

            var soleq = opt2.PerformOptimization();


            for (int i = 0; i < eq0.ParsCount; i++) {
                Assert.AreEqual(soleq.eq[eq0.ParNames[i]], TruePars[i], (p_max - p_min) / 100 * 30);
            }

        }

    }
}