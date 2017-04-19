using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;

namespace DoubleEnumGenetic.DetermOptimization {
    public class DownHill : SearchMethodBase {
        protected ChromosomeD _bs;
        public override ChromosomeD BestSolution {
            get {
                return _bs;
            }
        }

        public IList<ChromosomeD> currentPoints { get; private set; }
        public double lambda = 0.3, eps = 0.0001;

        public override void EndCurrentStep() {



            var jac = GetJacobian(currentPoints);
            var center = currentPoints.First(p => p.DopInfo == null);
            //if(center.Fitness <= BestSolution.Fitness) {
            //    var minShag = shagDict.Min(t => t.Value);
            //    foreach (var sh in shagDict) {
            //        shagDict[sh.Key] = minShag / 3;
            //    }
            //}
            var nextCenter = center.CloneWithoutFitness();
            foreach(var j in jac) {
                var step = lambda * j.Value;
                var maxStep = shagDict[j.Key] * (ShagNumber / 100);
                if (step > maxStep)
                    step = maxStep;
                nextCenter[j.Key] += step;
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
            currentPoints = GetPoints4Jacobian(center);
            return currentPoints;
        }
    }




}
