using GeneticSharp.Domain.Reinsertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;

namespace DoubleEnumGenetic {
    public class ReinsertionD_ParetoCritRank : ReinsertionBase {
        public int elitistCount;

        public ReinsertionD_ParetoCritRank(int elitistCount) : base(true,true) {
            this.elitistCount = elitistCount;
        }

        protected override IList<IChromosome> PerformSelectChromosomes(IPopulation population,IList<IChromosome> offspring,IList<IChromosome> parents) {
            var pareto = population.CurrentGeneration.Chromosomes.Cast<ChromosomeD>().Where(c => c.Fitness > -0.5).ToList();
            if(pareto.Count > elitistCount) {
                var m = ChromosomeD.GetCritDifferenceMatrix(pareto);
                var inds = ChromosomeD.GetUniquestGuysIndexes(m,elitistCount);
                var lst = new List<ChromosomeD>(pareto.Count);
                for(int i = 0; i < inds.Count; i++) {
                    lst.Add(pareto[inds[i]]);
                }
                pareto = lst;
            }
            return offspring.Concat(pareto.Cast<IChromosome>()).ToList();

        }
    }
}
