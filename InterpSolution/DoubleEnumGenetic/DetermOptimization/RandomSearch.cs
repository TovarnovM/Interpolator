using GeneticSharp.Domain.Randomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleEnumGenetic.DetermOptimization
{
    public class RandomSearch : SearchMethodBase
    {
        ChromosomeD currCenter;
        public override ChromosomeD BestSolution => currCenter;

        double[] arrow;
        List<ChromosomeD> pmArrow;
        int retryCountMax, retryCountCurr = 0;
        

        protected override void prepFunc() {
            base.prepFunc();
            arrow = new double[geneInfo.Count];
            retryCountMax = geneInfo.Count * 3;
            pmArrow = new List<ChromosomeD>(2);
            currCenter = Solutions.First();
        }

        public override void EndCurrentStep()
        {
            var plusArrow = pmArrow[0];
            var minusArrow = pmArrow[1];
            if (!plusArrow.Fitness.HasValue ||
               !minusArrow.Fitness.HasValue ||
               !currCenter.Fitness.HasValue)
                return;
            if(plusArrow.Fitness < currCenter.Fitness) {
                if(minusArrow.Fitness < currCenter.Fitness) {
                    if(retryCountCurr <= retryCountMax) {
                        retryCountCurr++;
                        return;
                    } else {
                        DecreaseShag();
                        retryCountCurr = 0;
                        return;
                    }
                } else {
                    currCenter = minusArrow;
                    //Solutions.Add(currCenter);
                    retryCountCurr = 0;
                    return;
                }
            } else {
                if (minusArrow.Fitness > currCenter.Fitness) {
                    DecreaseShag();
                    retryCountCurr = 0;
                    currCenter = plusArrow.Fitness > minusArrow.Fitness ? plusArrow : minusArrow;
                    //Solutions.Add(currCenter);
                    return;
                } else {
                    currCenter = plusArrow;
                    retryCountCurr = 0;
                    //Solutions.Add(currCenter);
                    return;
                }
            }
        }

        public override bool HasReached()
        {
            return (retryCountCurr == retryCountMax) && MinShagAlready;
        }

        public override IList<ChromosomeD> WhatCalculateNext()
        {
            RndArrow();
            FillpmArrow();
            if (currCenter.Fitness.HasValue) {
                return pmArrow;
            } else {
                return pmArrow.Concat(Enumerable.Repeat(currCenter,1)).ToList();
            }
        }

        void RndArrow() {
            for (int i = 0; i < arrow.Length; i++) {
                arrow[i] = RandomizationProvider.Current.GetDouble(-1d, 1d);
            }           
        }

        void FillpmArrow() {
            pmArrow.Clear();
            var plusArrow = currCenter.CloneWithoutFitness();
            var minusArrow = currCenter.CloneWithoutFitness();
            pmArrow.Add(plusArrow);
            pmArrow.Add(minusArrow);
            for (int i = 0; i < arrow.Length; i++) {
                var name = geneInfo[i].Name;
                double c = currCenter[name];
                plusArrow[name] = c + shagDict[name] * arrow[i];
                minusArrow[name] = c - (plusArrow[name] - c);
            }
            
        }
    }
}
