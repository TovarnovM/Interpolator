using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Chromosomes;

namespace DoubleEnumGenetic.DetermOptimization {
    public class DownHill : SearchMethodBase {
        protected ChromosomeD _bs, potentialCenter;
        public override ChromosomeD BestSolution {
            get {
                return _bs;
            }
        }

        protected override void prepFunc() {
            base.prepFunc();
            potentialCenter = Solutions.First();
            _bs = Solutions.First();
        }

        public IList<ChromosomeD> currentPoints { get; private set; }
        public double lambda = 0.3, eps = 0.00001;
        bool _hasReached = false;

        public override void EndCurrentStep() {
            if (currentPoints.Count == 1) {
                if(Object.ReferenceEquals(_bs,potentialCenter))
                    return;
                if (!potentialCenter.Fitness.HasValue || !_bs.Fitness.HasValue)
                    return;
                if (potentialCenter.Fitness < _bs.Fitness) {
                    _hasReached = !DecreaseShag();
                } else {
                    _bs = potentialCenter;
                    Solutions.Add(potentialCenter);
                }
                return;   
            }
                


            var jac = GetJacobian(currentPoints);
            var center = _bs;// currentPoints.First(p => p.DopInfo == null);
          
            var nextCenter = center.CloneWithoutFitness();
            foreach(var j in jac) {
                var step = lambda * j.Value;
                var maxStep = shagDict[j.Key] * (ShagNumber / 100);
                if (step > maxStep)
                    step = maxStep;
                nextCenter[j.Key] += step;
            }
            potentialCenter = nextCenter;
        }
        
        public override bool HasReached() {
            // return _hasReached;
            ChromosomeD last = null, prelast = null;
            for (int i = Solutions.Count - 1; i >= 0; i--) {
                if (!Solutions[i].Fitness.HasValue)
                    continue;
                if (last == null)
                    last = Solutions[i];
                else
                    prelast = Solutions[i];

                if (last != null && prelast != null)
                    break;

            }
            if (last == null || prelast == null)
                return MinShagAlready;
            return Math.Abs(last.Fitness.Value - prelast.Fitness.Value) < eps || MinShagAlready;
        }

        public override IList<ChromosomeD> WhatCalculateNext() {
            if (!potentialCenter.Fitness.HasValue) {
                currentPoints = Enumerable.Repeat(potentialCenter, 1).ToList();

            } else {
                currentPoints = GetPoints4Jacobian(_bs);
            }
                
            return currentPoints;
        }
    }




}
