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
using GeneticSharp.Infrastructure.Framework.Threading;

namespace EqOptimizer {


    public class EqO_genetic : EqO_TaskExecutor_fitness, IFitness {
        
        
        public int Popsize { get; set; } = 400;
        public int StagGenerNumber { get; set; } = 20; 



        public GeneticAlgorithm ga;

        public EqO_genetic(EquationBase Equation, MultyData Data, CriteriaBase Crit, double minPar = -100000, double maxPar = 100000) : base(Equation, Data, Crit, minPar, maxPar) {
        }

        public override (EquationBase eq, double crit) PerformOptimization() {
            //Popsize = 400;// Equation.ParsCount*Equation.ParsCount;
            ga = new GeneticAlgorithm(new Population(Popsize, Popsize + 10, GetNewChromo()), this, new TournamentSelection(), new CrossoverD(), new MutationD(true));
            ga.MutationProbability = 0.1f;
            ga.CrossoverProbability = 0.9f;

            var term = new FitnessStagnationTermination(StagGenerNumber);
            //var term2 = new FitnessThresholdTermination(-Data.MaxDiffY/4);

            //ga.Termination = new AndTermination(term,term2);
            ga.Termination = term;

            ga.Reinsertion = new Reinsertion_Elite(3);
            ga.TaskExecutor = taskEx;

            ga.Start();
            var be = ConvertFrom(ga.BestChromosome);

            //var sm = new DownHill();
            //sm.ShagNumber = 3000;
            //var opt = new Optimizator(ga.BestChromosome as ChromosomeD, this, sm,false);
            //opt.Start();
            //be = ConvertFrom(opt.BestChromosome);

            return (be, Crit.GetCriteria(be,Data));
        }
    }
}
