using GeneticSharp.Domain.Terminations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain;

namespace DoubleEnumGenetic.DetermSearch {
    class DetermTermination : TerminationBase {
        #region Fields
        private double m_lastFitness = double.NaN;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FitnessStagnationTermination"/> class.
        /// </summary>
        /// <remarks>
        /// The ExpectedStagnantGenerationsNumber default value is 100.
        /// </remarks>
        public DetermTermination()
        {
        }


        #endregion



        #region Methods
        /// <summary>
        /// Determines whether the specified geneticAlgorithm reached the termination condition.
        /// </summary>
        /// <returns>True if termination has been reached, otherwise false.</returns>
        /// <param name="geneticAlgorithm">The genetic algorithm.</param>
        protected override bool PerformHasReached(IGeneticAlgorithm geneticAlgorithm) {
            var ga = geneticAlgorithm as GeneticAlgorithm;
            if(ga == null)
                throw new Exception("Не то в Terminations");
            var pop = ga.Population as DetermPopulation;
            if(pop == null)
                throw new Exception("Не то в population");



            var bestFitness = pop.Solutions.Last().Fitness.Value;

            if(double.IsNaN(m_lastFitness)) {
                m_lastFitness = bestFitness;
                return false;
            }

            if(bestFitness > m_lastFitness) {
                m_lastFitness = bestFitness;
                return false;
            } else {
                m_lastFitness = bestFitness;
                return true;
            }            
        }
        #endregion
    }
}
