using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;

namespace MultiGenetic {
    class FitnesDE : IFitness {
        private List<CritInfo> Crits { get; set; } = new List<CritInfo>();

        public double Evaluate(IChromosome chromosome) {
            throw new NotImplementedException();
        }
    }
}
