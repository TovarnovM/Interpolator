using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;

namespace DoubleEnumGenetic.DetermOptimization {
    public class DownHill : SearchMethodBase {
        ChromosomeD _bs;
        public override ChromosomeD BestSolution {
            get {
                return _bs;
            }
        }

        public IList<ChromosomeD> currentPoints4Jacob { get; private set; }
        public double lambda = 0.3, eps = 0.0001;

        public override void EndCurrentStep() {
            var jac = GetJacobian(currentPoints4Jacob);
            var center = currentPoints4Jacob.First(p => p.DopInfo == null);
            var nextCenter = center.CloneWithoutFitness();
            foreach(var j in jac) {
                nextCenter[j.Key] += lambda * j.Value;
            }
            Solutions.Add(nextCenter);
            _bs = center;
        }
        
        public override bool HasReached() {
            ChromosomeD last = null, prelast = null;
            for(int i = Solutions.Count - 1; i >= 0; i--) {
                if(!Solutions[i].Fitness.HasValue)
                    continue;
                if(last == null)
                    last = Solutions[i];
                else
                    prelast = Solutions[i];

                if(last != null && prelast != null)
                    break;

            }
            if(last == null || prelast == null)
                return false;
            return Math.Abs(last.Fitness.Value - prelast.Fitness.Value) < eps;
        }

        public override IList<ChromosomeD> WhatCalculateNext() {
            var center = Solutions.Last();
            currentPoints4Jacob = GetPoints4Jacobian(center);
            return currentPoints4Jacob;
        }
    }




}
