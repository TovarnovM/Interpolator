using GeneticSharp.Domain.Crossovers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;

namespace DoubleEnumGenetic.DetermSearch {
    public class DetermCross : CrossoverBase {

        public DetermCross(int parentNumb) : base(parentNumb,1) {
        }

        protected override IList<IChromosome> PerformCross(IList<IChromosome> parents) {
            return Enumerable.Empty<IChromosome>().ToList();
        }
    }
}
