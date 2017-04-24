using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EqOptimizer.Criterias;
using EqOptimizer.Data;
using EqOptimizer.Equations;
using DoubleEnumGenetic.DetermOptimization;

namespace EqOptimizer {
    public class EqO_RandomSearch : EqO_TaskExecutor_fitness {
        public double minShag { get; set; } = 1e-7;

        public EqO_RandomSearch(EquationBase Equation, MultyData Data, CriteriaBase Crit, double minPar = -100000, double maxPar = 100000) : base(Equation, Data, Crit, minPar, maxPar) {
            Multithread = false;
        }

        public override (EquationBase eq, double crit) PerformOptimization() {
            var sm = new RandomSearch();
            var opt = new Optimizator(GetNewChromo(EquationInit), this, sm, Multithread);
            sm.minShag = minShag;
            opt.Start();
            var be = ConvertFrom(opt.BestChromosome);

            return (be, Crit.GetCriteria(be, Data));
        }
    }
}
