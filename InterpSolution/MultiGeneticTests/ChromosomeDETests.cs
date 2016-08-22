using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiGenetic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;

namespace MultiGenetic.Tests {
    [TestClass()]
    public class ChromosomeDETests {
        private ChromosomeDE cr1, cr2, cr3;
        [TestMethod(), TestInitialize()]
        public void ChromosomeDETest() {
            var gi = new List<IGeneInfo>();
            gi.Add(new DoubleRange("double1",0,7));
            gi.Add(new DoubleRange("double2",1,7));
            gi.Add(new DoubleRange("double3",-7,0));
            gi.Add(new DoubleRange("double4",-1,60));
            gi.Add(new StringRange("strRange1","sr10","sr11","sr12"));
            gi.Add(new StringRange("strRange2","sr20","sr21"));
            gi.Add(new StringRange("strRange3","sr30","sr31","sr32","sr33"));

            cr1 = new ChromosomeDE(gi);
            cr1[0] = 1d;
            cr1["double2"] = 2d;
            cr1[2] = 0d;
            cr1[3] = -1d;
            cr1[4] = 0;
            cr1["strRange2"] = "sr21";
            cr1["strRange3"] = 3;

            cr2 = cr1.CreateNew() as ChromosomeDE;
            cr3 = cr1.CreateNew() as ChromosomeDE;

            var crit1 = new CritInfo("fit1");
            var crit2 = new CritInfo("fit2",CritExtremum.fe_min,null,10d);
            var crit3 = new CritInfo("fit3");

            cr1.AddCrit(crit1,crit2,crit3);
            cr1.Crits["fit1"].Value = 1;
            cr1.Crits["fit2"].Value = 2;
            cr1.Crits["fit3"].Value = 3;
            cr2.AddCrit(cr1);
            cr3.AddCrit(cr1,true);

        }


        [TestMethod()]
        public void ParetoRelTest() {
            var cr1clone = cr1.Clone() as ChromosomeDE;
            Assert.IsTrue(ChromosomeDE.ParetoRel(cr1,cr1clone) == 0);

            cr1clone.Crits["fit1"].Value = 2;
            Assert.IsTrue(ChromosomeDE.ParetoRel(cr1,cr1clone) == -1);

            cr1clone.Crits["fit1"].Value = 0;
            Assert.IsTrue(ChromosomeDE.ParetoRel(cr1,cr1clone) == 1);

            cr1clone.Crits["fit2"].Value = 3;
            Assert.IsTrue(ChromosomeDE.ParetoRel(cr1,cr1clone) == 1);

            cr1clone.Crits["fit2"].Value = -3;
            Assert.IsTrue(ChromosomeDE.ParetoRel(cr1,cr1clone) == 0);
        }
    }
}