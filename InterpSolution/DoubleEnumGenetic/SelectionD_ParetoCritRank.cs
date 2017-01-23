using GeneticSharp.Domain.Selections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Randomizations;

namespace DoubleEnumGenetic {
    public class SelectionD_ParetoCritRank : SelectionBase {
        private ISelection selectionNext;

        public SelectionD_ParetoCritRank(ISelection selectionNext) : base(2) {
            this.selectionNext = selectionNext;
        }

        protected override IList<IChromosome> PerformSelectChromosomes(int number,Generation generation) {
            var lst = new List<ChromosomeD>(generation.Chromosomes.Cast<ChromosomeD>());
            int rank = 0;
            List<IChromosome> pareto = new List<IChromosome>();
            while(lst.Count > 0) {
                var par = ChromosomeD.Pareto(lst,true);
                if(rank == 0) {
                    pareto.AddRange(par);
                }
                foreach(var c in par) {                  
                    c.Fitness = c.ValidCrits() ? rank : rank-1;
                }
                rank--;
            }
            
            var parents = selectionNext.SelectChromosomes(number,generation);
            //var rndIndexes = RandomizationProvider.Current.GetUniqueInts(pareto.Count,0,parents.Count);
            //for(int i = 0; i < pareto.Count; i++) {
            //    parents[rndIndexes[i]] = pareto[i];
            //}

            return parents;
        }
    }
}
