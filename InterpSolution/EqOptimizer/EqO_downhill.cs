using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using EqOptimizer.Criterias;
using EqOptimizer.Data;
using EqOptimizer.Equations;
using DoubleEnumGenetic.DetermOptimization;
using DoubleEnumGenetic;

namespace EqOptimizer {
    public class EqO_downhill : EqO_TaskExecutor_fitness, IFitness {
        public double minShag { get; set; } = 1e-7;
        public double epsFitn { get; set; } = 1e-7;
        public double lambda = 0.3;

        public EqO_downhill(EquationBase Equation, MultyData Data, CriteriaBase Crit, double minPar = -100000, double maxPar = 100000) : base(Equation, Data, Crit, minPar, maxPar) {

        }

        public override (EquationBase eq, double crit) PerformOptimization() {
            var sm = new DownHill();
            var opt = new Optimizator(GetNewChromo(EquationInit), this, sm, Multithread);
            sm.minShag = minShag;
            sm.eps = epsFitn;
            sm.lambda = lambda;
            opt.Start();
            var be = ConvertFrom(opt.BestChromosome);

            return (be, Crit.GetCriteria(be, Data));
        }
    }
}
