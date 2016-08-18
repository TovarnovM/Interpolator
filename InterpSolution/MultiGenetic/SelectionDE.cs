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

        public void TryGetInPareto(IList<ChromosomeDE> pareto, ChromosomeDE candidate) {
            for (int i = pareto.Count - 1; i >= 0; i--) {
                var pr = pareto[i];
                switch (ChromosomeDE.ParetoRel(pr, candidate)) {
                    case 1: {
                            return;
                        }
                    case -1: {
                            pareto.RemoveAt(i);
                        }
                        break;
                    default:
                        break;
                }
            }
            pareto.Add(candidate);
        }

        public IList<ChromosomeDE> Pareto(IEnumerable<ChromosomeDE> all) {
            var par = new List<ChromosomeDE>();
            foreach (var chr in all) {
                TryGetInPareto(par, chr);
            }
            return par;
        }

        protected override IList<IChromosome> PerformSelectChromosomes(int number, Generation generation) {
            throw new NotImplementedException();
        }
    }
}
