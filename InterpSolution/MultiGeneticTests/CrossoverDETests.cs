using GeneticSharp.Domain.Chromosomes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiGenetic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;

namespace MultiGenetic.Tests {
    [TestClass()]
    public class CrossoverDETests {
        [TestMethod()]
        public void PerformCross4TestTest() {
            var gi= new List<IGeneInfo>();

            gi.Add(new GeneDoubleRange("1",1,2));
            gi.Add(new GeneDoubleRange("2",2,4));
            gi.Add(new GeneDoubleRange("3",3,6));
            gi.Add(new GeneDoubleRange("4",4,8));
            var p1 = new ChromosomeDE(gi);
            var p2 = new ChromosomeDE(gi);

            p1["1"] = 1;
            p1["2"] = 4;
            p1["3"] = 4;
            p1["4"] = 4;

            var g1 = new double[] { p1["1"],p1["2"],p1["3"],p1["4"] };

            p2["1"] = 1;
            p2["2"] = 4;
            p2["3"] = 4;
            p2["4"] = 5;

            var lstp = new List<IChromosome>(2);
            lstp.Add(p1);
            lstp.Add(p2);
            var cross = new CrossoverDE();
            var ch = cross.Cross(lstp);
            var ch1 = ch[0] as ChromosomeDE;
            var ch2 = ch[1] as ChromosomeDE;
            Assert.AreEqual(Convert.ToDouble(ch1["1"]),Convert.ToDouble(ch2["1"]));
            Assert.AreEqual(Convert.ToDouble(ch1["2"]),Convert.ToDouble(ch2["2"]));
            Assert.AreEqual(Convert.ToDouble(ch1["3"]),Convert.ToDouble(ch2["3"]));

            Assert.IsTrue((double)ch1["4"] <= 4.5 + 0.5 * 3.5);
            Assert.IsTrue((double)ch2["4"] <= 4.5 + 0.5 * 3.5);
        }
    }
}