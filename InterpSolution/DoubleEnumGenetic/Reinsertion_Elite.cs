using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Reinsertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (diff > 0) {
                var bestParents = parents.OrderByDescending(p => p.Fitness).Take(diff);

                foreach (var p in bestParents) {
                    offspring.Add(p);
                }
            }
            var elita = population.CurrentGeneration.Chromosomes.Where(c => c.Fitness.HasValue).OrderByDescending(c => c.Fitness).Take(eliteSurvCount).ToList();
            foreach (var c in elita) {
                offspring.Remove(c);
            }
            foreach (var c in elita) {
                offspring.Add(c);
            }
            return offspring;
        }
        #endregion
    }
}
