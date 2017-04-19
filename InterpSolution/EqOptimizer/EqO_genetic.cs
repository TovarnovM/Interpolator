using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EqOptimizer.Criterias;
using EqOptimizer.Data;
using EqOptimizer.Equations;
using DoubleEnumGenetic.DetermOptimization;
using GeneticSharp.Domain.Fitnesses;
using DoubleEnumGenetic;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Infrastructure.Threading;
using GeneticSharp.Domain.Terminations;

namespace EqOptimizer {
    public class EqO_genetic : EqOptimizerBase, IFitness {
        
        public IList<IGeneDE> GInfo { get; set; }
        public ChromosomeD GetNewChromo() {
            var c = new ChromosomeD(GInfo);
            return c;

        }

        public double Evaluate(IChromosome chromosome) {
            return -Crit.GetCriteria(ConvertFrom(chromosome), Data);
        }

        EquationBase ConvertFrom(IChromosome chromosome) {
            var c = chromosome as ChromosomeD;
            if (c == null)
                throw new Exception("Не та хромосома");
            var pars = new double[EquationInit.ParsCount];
            for (int i = 0; i < EquationInit.ParsCount; i++) {
                var pn = EquationInit.ParNames[i];
                pars[i] = c[pn];
            }

            return EquationInit.Clone(pars);
        }

        public GeneticAlgorithm ga;
        public EqO_genetic(EquationBase Equation,MultyData Data,CriteriaBase Crit, double minPar = -1e5, double maxPar = 1e5) : base(Equation,Data,Crit) {
            GInfo = new List<IGeneDE>(Equation.ParsCount);
            foreach (var pn in Equation.ParNames) {
                GInfo.Add(new GeneDoubleRange(pn,minPar,maxPar));
            }
            int popsize = 400;// Equation.ParsCount*Equation.ParsCount;
            ga = new GeneticAlgorithm(new Population(popsize,popsize+10,GetNewChromo()),this,new TournamentSelection(),new CrossoverD(),new MutationD());
            ga.MutationProbability = 0.2f;
            ga.CrossoverProbability = 0.7f;
            var term = new FitnessStagnationTermination(300);
            var term2 = new FitnessThresholdTermination(-Data.MaxDiffY/4);

            ga.Termination = new AndTermination(term,term2);

            ga.Reinsertion = new Reinsertion_Elite(10);
            ga.TaskExecutor = new SmartThreadPoolTaskExecutor();
        }

        public override (EquationBase eq, double crit) PerformOptimization() {
            ga.Start();
            var be = ConvertFrom(ga.BestChromosome);

            //var maxp = be.Pars.Average(p => Math.Abs(p));

            //double range = 0.2;
            //foreach (var gi_ in GInfo) {
            //    var gi = gi_ as GeneDoubleRange;
            //    if (gi == null)
            //        continue;
            //    gi.Left = be[gi.Name] - range * maxp;
            //    gi.Right = be[gi.Name] + range * maxp;
            //}
            //var sm = new DownHill();
            //sm.ShagNumber = 3000;
            //var opt = new Optimizator(ga.BestChromosome as ChromosomeD,this,sm);
            //opt.Start();

            
            return (be, Crit.GetCriteria(be,Data));
        }
    }
}
