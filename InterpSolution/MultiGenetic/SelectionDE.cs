using GeneticSharp.Domain.Selections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;

namespace MultiGenetic {
    public class SelectionDE : SelectionBase {
        public SelectionDE(int minNumberChromosomes) : base(minNumberChromosomes) {
        }



        protected override IList<IChromosome> PerformSelectChromosomes(int number, Generation generation) {
            throw new NotImplementedException();
        }
    }
}
