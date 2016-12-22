using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Randomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace DoubleEnumGenetic {
    public class CrossoverD : UniformCrossover {
        public double DCrossProb { get; set; }
        public double TailLength { get; set; } = 0.15;

        public CrossoverD(double dCrossProb = 0.5,double mixProbability = 0.5) : base((float)mixProbability) {
            DCrossProb = dCrossProb;
        }
        protected override IList<IChromosome> PerformCross(IList<IChromosome> parents) {
            var result = base.PerformCross(parents);

            if(result[0] is ChromosomeD && result[1] is ChromosomeD) {
                var child1 = result[0] as ChromosomeD;
                var child2 = result[1] as ChromosomeD;
                for(int i = 0; i < child1.GInfoDouble.Count; i++) {
                    if(child1.GInfoDouble[i] is GeneDoubleRange) {
                        var gi = child1.GInfoDouble[i] as GeneDoubleRange;
                        var x1 = Min(
                            (double)child1.GetGene(i).Value,
                            (double)child2.GetGene(i).Value);
                        var x2 = Max(
                            (double)child1.GetGene(i).Value,
                            (double)child2.GetGene(i).Value);
                        var delta = Abs(x2 - x1);
                        var tail = delta * TailLength;
                        var xm = 0.5 * (x1 + x2);
                        var sko = (delta + 2 * tail) / 6;
                        if(RandomizationProvider.Current.GetDouble() <= DCrossProb) {
                            child1.ReplaceGene(i,new Gene(gi.GetRandValue_Norm(xm,sko)));
                        }
                        if(RandomizationProvider.Current.GetDouble() <= DCrossProb) {
                            child2.ReplaceGene(i,new Gene(gi.GetRandValue_Norm(xm,sko)));
                        }
                    }
                }
            }

            return result;
        }

        public IList<IChromosome> PerformCross4Test(IList<IChromosome> parents) {
            return PerformCross(parents);
        }


    }
}
