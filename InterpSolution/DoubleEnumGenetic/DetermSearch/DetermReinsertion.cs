using GeneticSharp.Domain.Reinsertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;

namespace DoubleEnumGenetic.DetermSearch {
    class DetermReinsertion : ReinsertionBase {
        public double DeltaShag = 4d;

        public DetermReinsertion() : base(true,true) {
        }

        protected override IList<IChromosome> PerformSelectChromosomes(IPopulation population,IList<IChromosome> offspring,IList<IChromosome> parents) {
            var pop = population as DetermPopulation;
            var par = parents.Cast<ChromosomeD>().ToList();
            if(pop == null || par == null || par.Count != parents.Count)
                throw new Exception("Чет не то в популяции");

            var center = par.First(p => p.DopInfo == null);
            var jac = pop.geneInfo
                .ToDictionary(
                    gi => gi.Name,
                    gi => new JacHelper(center, gi.Name)
                );

            
            foreach(var p in par) {
                if(p.DopInfo == null)
                    continue;
                var dopInfo = p.DopInfo as DopInfoClass;
                if(dopInfo == null)
                    throw new Exception("Чет не то в хромосомах популяции");
                jac[dopInfo.Name].UpdateLR(p);

            }

            foreach(var j in jac.Values) {
                j.dfdx = j.GetdFdx();
            }

            var norm = jac.Values.Sum(jh => jh.dfdx * jh.dfdx);

            foreach(var j in jac.Values) {
                j.dfdx /= norm; 
            }

            var centerNext = center.CloneWithoutFitness();
            foreach(var j in jac.Values) {
                var val = centerNext[j.paramName] + DeltaShag * pop.shagDict[j.paramName];
                centerNext[j.paramName] = val;
            }

            return pop.GetNeibsPlusCenter(centerNext).Cast<IChromosome>().ToList();


        }

        class JacHelper {
            double leftX, rightX, leftF, rightF;
            public string paramName;
            public JacHelper(ChromosomeD center, string paramName) {
                this.paramName = paramName;
                leftX = center[paramName];
                rightX = leftX;
                leftF = center.Fitness.Value;
                rightF = leftF;
            }
            
            public void UpdateLR(ChromosomeD chr) {
                var x = chr[paramName];
                var f = chr.Fitness.Value;
                if(x < leftX) {
                    leftX = x;
                    leftF = f;
                    return;
                }
                    
                if(x > rightX) {
                    rightX = x;
                    rightF = f;
                    return;
                }
            }

            public double DeltaX {
                get {
                    return rightX - leftX;
                }
            }
            public double DeltaF {
                get {
                    return rightF - rightF;
                }
            }
            public double GetdFdx() {
                if(Math.Abs(DeltaX) < 1E-10)
                    return 0;
                return DeltaF / DeltaX;
            }
            public double dfdx;
        }
    }
}
