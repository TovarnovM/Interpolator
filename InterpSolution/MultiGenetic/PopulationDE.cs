using GeneticSharp.Domain.Populations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;

namespace MultiGenetic {
    public class PopulationDE : Population {
        public PopulationDE(int minSize, int maxSize, IChromosome adamChromosome) : base(minSize, maxSize, adamChromosome) {
        }
    }
}
