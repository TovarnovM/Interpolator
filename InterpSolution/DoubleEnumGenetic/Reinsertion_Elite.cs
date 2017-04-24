using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Domain.Reinsertions;
using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace DoubleEnumGenetic {
    public class Reinsertion_Elite : ReinsertionBase {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneticSharp.Domain.Reinsertions.ElitistReinsertion"/> class.
        /// </summary>
        public Reinsertion_Elite(int eliteSurvCount = 10) : base(false,true) {
            this.eliteSurvCount = eliteSurvCount;
        }

        public int eliteSurvCount { get; set; }
        #endregion

        MutationD muttation = new MutationD(true);

        #region Methods
        /// <summary>
        /// Selects the chromosomes which will be reinserted.
        /// </summary>
        /// <returns>The chromosomes to be reinserted in next generation..</returns>
        /// <param name="population">The population.</param>
        /// <param name="offspring">The offspring.</param>
        /// <param name="parents">The parents.</param>
        protected override IList<IChromosome> PerformSelectChromosomes(IPopulation population,IList<IChromosome> offspring,IList<IChromosome> parents) {
            var diff = population.MinSize - offspring.Count;
            var elita = population.CurrentGeneration.Chromosomes.Where(c => c.Fitness.HasValue).OrderByDescending(c => c.Fitness).Take(eliteSurvCount).ToList();

            if (diff > eliteSurvCount) {
                var bestParents = parents.OrderByDescending(p => p.Fitness).Take(diff).Skip(eliteSurvCount);
                
                foreach (var p in bestParents) {
                    offspring.Add(p);
                }
            }

            var diffMatr = ChromosomeD.GetGeneDifferenceMatrix(offspring.Cast<ChromosomeD>().ToList());

            int zcount = ZeroCount(diffMatr);

            var uniquestInd = ChromosomeD.GetUniquestGuysIndexes(diffMatr, offspring.Count - zcount);
            var patheticClones = Enumerable.Range(0, offspring.Count).Except(uniquestInd).ToList();
            double renewProb = 0.1;
            float mutprob = 1 * 1f / offspring[0].GetGenes().Length;
            var offspr2 = offspring.ToList();
            for (int i = 0; i < patheticClones.Count; i++) {
                if (RandomizationProvider.Current.GetDouble() < renewProb) {
                    offspr2[patheticClones[i]] = offspr2[patheticClones[i]].CreateNew();
                    continue;
                }

                var mutationVictum = offspr2[patheticClones[i]];
                muttation.Mutate(mutationVictum, mutprob);
            }

            if(elita.Count + offspr2.Count > population.MinSize) {
                int l = elita.Count + offspr2.Count - population.MinSize;
                var goodBye = RandomizationProvider.Current.GetUniqueInts(l, 0, offspr2.Count);
                Array.Sort(goodBye);
                for (int i = goodBye.Length - 1; i >= 0; i--) {
                    offspr2.RemoveAt(goodBye[i]);
                }

            }


            foreach (var c in elita) {
                offspr2.Add(c);
            }

            for (int i = offspr2.Count-1; i < population.MinSize; i++) {
                offspr2.Add(offspr2.First().CreateNew());
            }

            return offspr2;
        }

        int ZeroCount(MatrixD matr) {
            int count = 0;
            for (int i = 0; i < matr.Rows; i++) {
                for (int j = i+1; j < matr[i].Length; j++) {
                    if (Abs(matr[i, j]) < 1E-10)
                        count++;

                }
            }
            return count;
        }
        #endregion
    }
}
