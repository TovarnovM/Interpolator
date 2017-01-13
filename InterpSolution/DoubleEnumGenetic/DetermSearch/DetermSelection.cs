using GeneticSharp.Domain.Selections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;

namespace DoubleEnumGenetic.DetermSearch {
    public class DetermSelection : SelectionBase {
        public DetermSelection() : base(1) {
        }

        protected override IList<IChromosome> PerformSelectChromosomes(int number,Generation generation) {
            return generation.Chromosomes;
        }
    }
}
